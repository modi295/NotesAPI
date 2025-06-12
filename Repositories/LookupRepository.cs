using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly NotesApplicationContext _context;

        public LookupRepository(NotesApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lookup>> GetAllAsync()
        {
            return await _context.Lookups.ToListAsync();
        }
        public async Task<Lookup?> GetByIdAsync(string typeId) =>
            await _context.Lookups.FindAsync(typeId);

        public async Task<Lookup> CreateAsync(Lookup lookup)
        {
            _context.Lookups.Add(lookup);
            await _context.SaveChangesAsync();
            return lookup;
        }

        public async Task<Lookup?> UpdateAsync(string typeId, Lookup updatedLookup)
        {
            var existing = await _context.Lookups.FindAsync(typeId);
            if (existing == null) return null;

            existing.TypeCode = updatedLookup.TypeCode;
            existing.TypeName = updatedLookup.TypeName;
            existing.UpdatedBy = updatedLookup.UpdatedBy;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string typeId)
        {
            var existing = await _context.Lookups.FindAsync(typeId);
            if (existing == null) return false;

            _context.Lookups.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}