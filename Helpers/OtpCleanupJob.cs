using Quartz;
using NotesAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace NotesAPI.Helpers
{
    public class OtpCleanupJob : IJob
    {
        private readonly NotesApplicationContext _context;

        public OtpCleanupJob(NotesApplicationContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var expiredOtps = await _context.Otps
                .Where(o => o.ExpiredAt < DateTime.UtcNow)
                .ToListAsync();

            if (expiredOtps.Any())
            {
                _context.Otps.RemoveRange(expiredOtps);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Quartz] Deleted {expiredOtps.Count} expired OTPs at {DateTime.Now}");
            }
        }
    }

    public interface IOtpGenerator
    {
        string GenerateOtp(int length = 6);
    }

    public class OtpGenerator : IOtpGenerator
    {
        public string GenerateOtp(int length = 6)
        {
            var random = new Random();
            return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10)));
        }
    }


}
