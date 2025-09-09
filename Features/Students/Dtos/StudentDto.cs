namespace NotesAPI.Features.Students.Dtos
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}