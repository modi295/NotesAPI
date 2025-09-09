namespace NotesAPI.Repositories
{
    public interface IBotService
    {
        Task<string> GetResponseAsync(string message);
    }
}