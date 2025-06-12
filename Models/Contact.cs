namespace NotesAPI.Models;

public partial class Contact  : AuditableEntity
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Comment { get; set; } = null!;
}
