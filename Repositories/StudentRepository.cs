using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly NotesApplicationContext _context;
        public StudentRepository(NotesApplicationContext context) => _context = context;

        public async Task<List<Student>> GetAllAsync() => await _context.Students.ToListAsync();

        public async Task<Student?> GetByIdAsync(int id) => await _context.Students.FindAsync(id);

        public async Task<int> AddAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student.Id;
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await GetByIdAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }
    }

}