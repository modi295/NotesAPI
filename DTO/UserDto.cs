namespace NotesAPI.DTO
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public double? Latitude { get; set; }      // nullable because it might not be sent
        public double? Longitude { get; set; }
    }

    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumberCode { get; set; }
        public string? PhoneNumber { get; set; }

        public object? ProfilePicture { get; set; }

        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? University { get; set; }
        public string? College { get; set; }
        public string? Active { get; set; }
        public string? Remark { get; set; }
        public string Role { get; set; } = null!;
        public string? AddedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumberCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? University { get; set; }
        public string? College { get; set; }
        public string? Active { get; set; }
        public string? Remark { get; set; }
    }

    public class EmailRequestDto
    {
        public string Email { get; set; } = null!;
    }

    public class OtpVerifyRequestDto
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
    public class GoogleCredentialRequest
    {
        public string Credential { get; set; } = null!;
    }
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LocationInfo
    {
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string Query { get; set; } = null!;
        public string Timezone { get; set; } = null!;
    }
}
