using MediatR;
using NotesAPI.Features.Students.Commands;
using NotesAPI.Models;
using NotesAPI.Repositories;

namespace NotesAPI.Features.Students.Handlers
{
    public class CreateStudentHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IStudentRepository _repository;
        public CreateStudentHandler(IStudentRepository repository) => _repository = repository;

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            var student = new Student { Name = request.Name, School = request.School, Age = request.Age };
            return await _repository.AddAsync(student);
        }
    }
}