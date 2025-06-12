namespace NotesAPI.Models;

public partial class Otp
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Otp1 { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
