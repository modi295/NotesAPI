using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public class SupportRepository : ISupportRepository
    {
        private readonly NotesApplicationContext _context;

        public SupportRepository(NotesApplicationContext context)
        {
            _context = context;
        }

        public async Task<Support?> GetSupportAsync()
        {
            return await _context.Supports.OrderBy(s => s.Id).FirstOrDefaultAsync();
        }
    }
}
