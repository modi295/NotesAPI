
using MediatR;
using NotesAPI.Features.Students.Dtos;

namespace NotesAPI.Features.Students.Queries
{
    public class GetAllStudentsQuery : IRequest<List<StudentDto>> { }

}