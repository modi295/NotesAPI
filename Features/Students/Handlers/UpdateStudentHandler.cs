using MediatR;
using NotesAPI.Features.Students.Commands;
using NotesAPI.Repositories;

namespace NotesAPI.Features.Students.Handlers
{
    public class UpdateStudentHandler : IRequestHandler<UpdateStudentCommand, Unit>
    {
        private readonly IStudentRepository _repository;
        public UpdateStudentHandler(IStudentRepository repository) => _repository = repository;

        public async Task<Unit> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _repository.GetByIdAsync(request.Id);
            if (student == null) throw new KeyNotFoundException("Student not found");

            student.Name = request.Name;
            student.School = request.School;
            student.Age = request.Age;

            await _repository.UpdateAsync(student);
            return Unit.Value;
        }
    }
}