using MediatR;

namespace NotesAPI.Features.Students.Commands
{
    public class UpdateStudentCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public int Age { get; set; }
    }

}