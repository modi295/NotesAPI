using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public interface IOtpRepository
    {
        Task AddOtpAsync(Otp otp);
        Task<Otp?> GetValidOtpAsync(string email, string otpCode);
        void RemoveOtp(Otp otp);
        Task SaveChangesAsync();
    }
}