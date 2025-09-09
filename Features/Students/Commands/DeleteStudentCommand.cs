using MediatR;

namespace NotesAPI.Features.Students.Commands
{
    public class DeleteStudentCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public DeleteStudentCommand(int id) => Id = id;
    }
}