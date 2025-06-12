using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public interface ILookupRepository
    {
        Task<IEnumerable<Lookup>> GetAllAsync();
        Task<Lookup?> GetByIdAsync(string typeId);
        Task<Lookup> CreateAsync(Lookup lookup);
        Task<Lookup?> UpdateAsync(string typeId, Lookup updatedLookup);
        Task<bool> DeleteAsync(string typeId);
    }
}