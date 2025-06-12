using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public interface ISupportRepository
    {
        Task<Support?> GetSupportAsync();
    }
}
