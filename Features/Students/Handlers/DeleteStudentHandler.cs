using MediatR;
using NotesAPI.Features.Students.Commands;
using NotesAPI.Repositories;

namespace NotesAPI.Features.Students.Handlers
{
    public class DeleteStudentHandler : IRequestHandler<DeleteStudentCommand, Unit>
    {
        private readonly IStudentRepository _repository;
        public DeleteStudentHandler(IStudentRepository repository) => _repository = repository;

        public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            await _repository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}