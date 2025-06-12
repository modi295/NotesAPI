using Microsoft.AspNetCore.Mvc;
using NotesAPI.DTO;
using NotesAPI.Repositories;

namespace NotesAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]

    [Route("api")]
    [Route("api/v{version:apiVersion}")]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<UserController> _logger;


        public ContactController(IContactRepository contactRepository, ILogger<UserController> logger)
        {
            _contactRepository = contactRepository;
            _logger = logger;
        }

        // /api/contact
        [HttpPost("contact")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> ContactV1([FromBody] ContactRequestDto request)
        {
            try
            {
                var contactId = await _contactRepository.SubmitContactAsync(request);
                _logger.LogInformation("User '{Email}' logged in with OTP at {Time}.", request.Email, DateTime.Now);

                return Created("", new { userId = contactId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // /api/v2/contact
        [HttpPost("contact")]
        [MapToApiVersion("2.0")]
        public IActionResult ContactV2([FromBody] ContactRequestDto request)
        {
            // v2 logic here
            return Ok(new
            {
                message = "Handled by Contact API v2",
                contact = request
            });
        }
    }
}
