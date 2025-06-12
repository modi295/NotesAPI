namespace NotesAPI.DTO
{
    public class ContactRequestDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Comment { get; set; } = null!;
    }
}