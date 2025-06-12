using NotesAPI.DTO;
using NotesAPI.Helpers;
using NotesAPI.Models;
using NotesAPI.Repositories;
using System.Net.Mail;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NotesAPI.Validator;
using AutoMapper;

namespace NotesAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IConfiguration _config;
        private readonly IOtpGenerator _otpGenerator;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;


        public UserController(IOtpGenerator otpGenerator, IConfiguration config, IUserRepository userRepository, IOtpRepository otpRepository, IMemoryCache memoryCache, ILogger<UserController> logger, IMapper mapper)
        {
            _otpGenerator = otpGenerator;
            _config = config;
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _memoryCache = memoryCache;
            _logger = logger;
            _mapper = mapper;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var validator = new RegisterRequestDtoValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    var errorResponse = validationResult.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Error = e.ErrorMessage
                    });
                    return BadRequest(new { Errors = errorResponse });
                }

                var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User already exists" });
                }
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = hashedPassword,
                    Active = "N",
                };

                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();

                // Generate token for email verification
                var token = JwtTokenHelper.GenerateToken(user, _config);

                var verificationLink = $"http://192.168.2.39:3000/verify-email/{token}";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Email:User"]!),
                    Subject = "Note Marketplace - Email Verification",
                    Body = $@"
                <p>Hello {user.FirstName},</p>
                <p>Thank you for signing up with us. Please click the link below to verify your email address:</p>
                <a href=""{verificationLink}"">Verify Email</a>
                <p>Regards,<br>Notes Marketplace</p>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(user.Email);

                var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                {
                    EnableSsl = false, // Change to true for production
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true
                };

                await smtpClient.SendMailAsync(mailMessage);

                return StatusCode(201, new { message = "User registered successfully. Please check your email for verification." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Registration failed", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email and password are required" });
            }

            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            if (user.Active == "N")
            {
                return Unauthorized(new { error = "Your ID has been deactivated. Please contact Admin." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isPasswordValid)
            {
                return Unauthorized(new { error = "Invalid password" });
            }
            string location = "N/A";
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                location = await GeoLocationHelper.GetLocationFromLatLngNominatimAsync(request.Latitude.Value, request.Longitude.Value);
            }

            _logger.LogInformation(
                "User '{Email}' logged in with Password at {Time}. Machine: {MachineName}, OS: {OS}, IP: {IPAddress}, Latitude: {Latitude}, Longitude: {Longitude}, Location: {Location}",
                request.Email,
                DateTime.Now,
                Environment.MachineName,
                System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
                request.Latitude?.ToString() ?? "N/A",
                request.Longitude?.ToString() ?? "N/A",
                location
            );

            _logger.LogInformation("User '{Email}' logged in with Password at {Time}. Machine: {MachineName}, OS: {OS}, IP: {IPAddress}", request.Email, DateTime.Now, Environment.MachineName, System.Runtime.InteropServices.RuntimeInformation.OSDescription, HttpContext.Connection.RemoteIpAddress?.ToString());

            var tokenString = JwtTokenHelper.GenerateToken(user, _config);
            return Ok(new { token = tokenString, role = user.Role });
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            const string cacheKey = "userList";

            if (!_memoryCache.TryGetValue(cacheKey, out List<User>? users))
            {
                _logger.LogInformation("Cache miss for key '{CacheKey}'. Querying database...", cacheKey);

                var usersFromDb = await _userRepository.GetAllUsersAsync();
                users = usersFromDb?.ToList() ?? new List<User>();

                if (users.Count == 0)
                {
                    _logger.LogWarning("No users found in database when fetching with cache key '{CacheKey}'.", cacheKey);
                    return NotFound("No user found");
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    SlidingExpiration = TimeSpan.FromSeconds(30),
                    Size = 1024
                };

                _memoryCache.Set(cacheKey, users, cacheEntryOptions);

                _logger.LogInformation("User list cached with key '{CacheKey}' for 1 minute.", cacheKey);
            }
            else
            {
                _logger.LogInformation("Cache hit for key '{CacheKey}'. Returning cached users.", cacheKey);
            }

            return Ok(users);
        }

        [HttpGet("users/{email}")]
        public async Task<ActionResult<UserResponseDTO>> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound("No User Found");
            }

            var userDto = _mapper.Map<UserResponseDTO>(user);
            return Ok(userDto);
        }

        [HttpPut("users/{email}")]
        public async Task<IActionResult> UpdateUser(string email, [FromForm] UpdateUserDto dto, IFormFile? profilePicture)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Apply updates from DTO using AutoMapper
            _mapper.Map(dto, user);

            // Handle profile picture if present
            if (profilePicture != null && profilePicture.Length > 0)
            {
                using var ms = new MemoryStream();
                await profilePicture.CopyToAsync(ms);
                user.ProfilePicture = ms.ToArray();
            }

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { message = "User updated successfully" });
        }

        [HttpPost("sendOtp")]
        public async Task<IActionResult> SendOtpForLogin([FromBody] EmailRequestDto request)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                if (user.Active == "N")
                    return StatusCode(403, new { error = "Your ID has been deactivated. Please contact Admin." });

                var otp = _otpGenerator.GenerateOtp(6);
                var expiredAt = DateTime.UtcNow.AddMinutes(5);

                var otpEntry = new Otp
                {
                    Email = request.Email,
                    Otp1 = otp,
                    ExpiredAt = expiredAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _otpRepository.AddOtpAsync(otpEntry);
                await _otpRepository.SaveChangesAsync();

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Email:User"]!),
                    Subject = "Your OTP for Login",
                    Body = $"Your OTP is: {otp}. It will expire in 5 minutes.",
                    IsBodyHtml = false
                };
                mailMessage.To.Add(request.Email);

                var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                {
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true
                };

                await smtpClient.SendMailAsync(mailMessage);

                return StatusCode(201, new { Message = "Mail Sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to send OTP", details = ex.Message });
            }
        }

        [HttpPost("verifyOtp")]
        public async Task<IActionResult> VerifyOtpLogin([FromBody] OtpVerifyRequestDto request)
        {
            try
            {
                var otpRecord = await _otpRepository.GetValidOtpAsync(request.Email, request.Otp);

                if (otpRecord == null)
                    return BadRequest(new { error = "Invalid or expired OTP" });

                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                if (user == null)
                    return NotFound(new { error = "User not found" });

                if (user.Active == "N")
                    return StatusCode(403, new { error = "Your ID has been deactivated. Please contact Admin." });

                _otpRepository.RemoveOtp(otpRecord);
                await _otpRepository.SaveChangesAsync();
                _logger.LogInformation("User '{Email}' logged in with OTP at {Time}.", request.Email, DateTime.Now);

                var tokenString = JwtTokenHelper.GenerateToken(user, _config);
                return Ok(new { token = tokenString, role = user.Role });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to verify OTP", details = ex.Message });
            }
        }

        [HttpPost("auth/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleCredentialRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Authentication:Google:ClientId"] }
                });

                var email = payload.Email;
                var name = payload.Name;

                var user = await _userRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    var fullName = name?.Trim() ?? "";
                    var nameParts = fullName.Split(' ', 2);
                    user = new User
                    {
                        Email = email,
                        FirstName = nameParts.Length > 0 ? nameParts[0] : "",
                        LastName = nameParts.Length > 1 ? nameParts[1] : "",
                        Role = "User",
                        Active = "Y",
                        Password = BCrypt.Net.BCrypt.HashPassword("Google@123")
                    };
                    await _userRepository.AddUserAsync(user);
                    await _userRepository.SaveChangesAsync();
                }
                else if (user.Active != "Y")
                {
                    return StatusCode(403, new { error = "Your ID has been deactivated. Please contact Admin." });
                }

                _logger.LogInformation("User '{Email}' logged in with Google Verfification at {Time}.", email, DateTime.Now);

                var tokenString = JwtTokenHelper.GenerateToken(user, _config);
                return Ok(new { token = tokenString, role = user.Role });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Google authentication failed", details = ex.Message });
            }
        }
    }
}