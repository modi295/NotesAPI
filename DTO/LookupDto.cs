
namespace NotesAPI.DTO
{
    public class CreateLookupDto
    {
        public string TypeId { get; set; } = null!;
        public string? TypeCode { get; set; }
        public string? TypeName { get; set; }
    }

    public class UpdateLookupDto
    {
        public string? TypeCode { get; set; }
        public string? TypeName { get; set; }
    }
}