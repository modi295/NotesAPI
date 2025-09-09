using MediatR;
using NotesAPI.Features.Students.Dtos;
using NotesAPI.Features.Students.Queries;
using NotesAPI.Repositories;

namespace NotesAPI.Features.Students.Handlers
{
   public class GetAllStudentsHandler : IRequestHandler<GetAllStudentsQuery, List<StudentDto>>
{
    private readonly IStudentRepository _repository;
    public GetAllStudentsHandler(IStudentRepository repository) => _repository = repository;

    public async Task<List<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _repository.GetAllAsync();
        return students.Select(s => new StudentDto { Id = s.Id, Name = s.Name, School = s.School, Age = s.Age }).ToList();
    }
}
}