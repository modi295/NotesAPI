namespace NotesAPI.Models;

public partial class User : AuditableEntity
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumberCode { get; set; }

    public string? PhoneNumber { get; set; }

    public byte[]? ProfilePicture { get; set; }

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

    public virtual ICollection<BuyerNote> BuyerNotes { get; set; } = new List<BuyerNote>();

    public virtual ICollection<DownloadNote> DownloadNotes { get; set; } = new List<DownloadNote>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual ICollection<SoldNote> SoldNotes { get; set; } = new List<SoldNote>();
}
