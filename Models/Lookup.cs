
namespace NotesAPI.Models;

public partial class Lookup  : AuditableEntity
{
    public string TypeId { get; set; } = null!;

    public string? TypeCode { get; set; }

    public string? TypeName { get; set; }

}
