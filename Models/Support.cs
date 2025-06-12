namespace NotesAPI.Models;

public partial class Support : AuditableEntity
{
    public int Id { get; set; }

    public string? SupportEmail { get; set; }

    public string? SupportPhone { get; set; }

    public string? EmailAddress { get; set; }

    public string? FacebookUrl { get; set; }

    public string? TwitterUrl { get; set; }

    public string? LinkedinUrl { get; set; }

    public string? NoteImage { get; set; }

    public string? ProfilePicture { get; set; }
}
