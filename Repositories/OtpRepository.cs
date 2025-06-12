using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly NotesApplicationContext _context;

        public OtpRepository(NotesApplicationContext context)
        {
            _context = context;
        }

        public async Task AddOtpAsync(Otp otp)
        {
            await _context.Otps.AddAsync(otp);
        }

        public async Task<Otp?> GetValidOtpAsync(string email, string otpCode)
        {
            return await _context.Otps
                .Where(o => o.Email == email && o.Otp1 == otpCode && o.ExpiredAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public void RemoveOtp(Otp otp)
        {
            _context.Otps.Remove(otp);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}