using NotesAPI.DTO;

namespace NotesAPI.Repositories
{
    public interface IContactRepository
    {
        Task<int> SubmitContactAsync(ContactRequestDto request);
    }
}
