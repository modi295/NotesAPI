using MediatR;
using NotesAPI.Features.Students.Dtos;

namespace NotesAPI.Features.Students.Queries
{
    public class GetStudentByIdQuery : IRequest<StudentDto?>
    {
        public int Id { get; set; }
        public GetStudentByIdQuery(int id) => Id = id;
    }
}