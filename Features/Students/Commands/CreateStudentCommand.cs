using MediatR;

namespace NotesAPI.Features.Students.Commands
{
    public class CreateStudentCommand : IRequest<int>
    {
        public string Name { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}