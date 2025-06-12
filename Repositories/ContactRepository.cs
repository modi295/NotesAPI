using NotesAPI.DTO;
using NotesAPI.Models;
using System.Net.Mail;

namespace NotesAPI.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly NotesApplicationContext _context;
        private readonly IConfiguration _config;

        public ContactRepository(NotesApplicationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<int> SubmitContactAsync(ContactRequestDto request)
        {
            var contact = new Contact
            {
                FullName = request.FullName,
                Email = request.Email,
                Subject = request.Subject,
                Comment = request.Comment
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
            {
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Email:User"]!),
                Subject = $"{contact.FullName} - Query",
                Body = $"Hello Admin,\n\n{contact.Comment}\n\nRegards,\n{contact.FullName}",
                IsBodyHtml = false
            };
            mailMessage.To.Add(_config["Email:User"]!);

            await smtpClient.SendMailAsync(mailMessage);

            return contact.Id;
        }
    }
}
