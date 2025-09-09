using MediatR;
using NotesAPI.Features.Students.Dtos;
using NotesAPI.Features.Students.Queries;
using NotesAPI.Repositories;

namespace NotesAPI.Features.Students.Handlers
{
    public class GetStudentByIdHandler : IRequestHandler<GetStudentByIdQuery, StudentDto?>
{
    private readonly IStudentRepository _repository;
    public GetStudentByIdHandler(IStudentRepository repository) => _repository = repository;

    public async Task<StudentDto?> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.Id);
        if (student == null) return null;
        return new StudentDto { Id = student.Id, Name = student.Name, School = student.School, Age = student.Age };
    }
}
}