using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class BuyerController : ControllerBase
    {
        private readonly NotesApplicationContext _context;
        private readonly IConfiguration _config;

        public BuyerController(NotesApplicationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("buyernotes/{email}")]
        public async Task<IActionResult> GetBuyerNotes(string email)
        {
            try
            {
                var buyerNotes = await _context.BuyerNotes
                    .Where(b => b.PurchaseEmail == email)
                    .ToListAsync();

                if (buyerNotes == null || !buyerNotes.Any())
                {
                    return NotFound(new { message = "No download notes found for this user" });
                }

                return Ok(buyerNotes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("buyernotes/{id}")]
        public async Task<IActionResult> UpdateBuyerNote(int id, [FromBody] BuyerNote updatedData)
        {
            try
            {
                var existingNote = await _context.BuyerNotes.FindAsync(id);
                if (existingNote == null)
                {
                    return NotFound(new { success = false, message = "Buyer note not found" });
                }

                var seller = await _context.Users.FirstOrDefaultAsync(u => u.Email == updatedData.PurchaseEmail);
                var buyer = await _context.Users.FirstOrDefaultAsync(u => u.Email == updatedData.BuyerEmail);

                var sellerName = (seller?.FirstName + " " + seller?.LastName)?.Trim() ?? "Seller";
                var buyerName = (buyer?.FirstName + " " + buyer?.LastName)?.Trim() ?? "Buyer";

                // Update the fields
                existingNote.Email = updatedData.Email;
                existingNote.NoteId = updatedData.NoteId;
                existingNote.NoteTitle = updatedData.NoteTitle;
                existingNote.Category = updatedData.Category;
                existingNote.SellFor = updatedData.SellFor;
                existingNote.SellPrice = updatedData.SellPrice;
                existingNote.PurchaseEmail = updatedData.PurchaseEmail;
                existingNote.BuyerEmail = updatedData.BuyerEmail;
                existingNote.ApproveFlag = updatedData.ApproveFlag;
                existingNote.UpdatedBy = updatedData.UpdatedBy;
                existingNote.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Email sending logic
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Email:User"]!),
                    Subject = $"{sellerName} Allows you to download a note",
                    Body = $@"
                Hello {buyerName},

                We would like to inform you that {sellerName} allows you to download a note.
                Please login and check the 'My Downloads' tab to download the particular note.

                Regards,
                Notes Marketplace",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(updatedData.BuyerEmail!);

                using var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                {
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("Approval email sent successfully");

                return Ok(new { success = true, data = existingNote });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("buyernotes")]
        public async Task<IActionResult> PostBuyerNote([FromBody] BuyerNote request)
        {
            try
            {
                var newNote = new BuyerNote
                {
                    Email = request.Email,
                    NoteId = request.NoteId,
                    NoteTitle = request.NoteTitle,
                    Category = request.Category,
                    SellFor = request.SellFor,
                    SellPrice = request.SellPrice,
                    PurchaseEmail = request.PurchaseEmail,
                    BuyerEmail = request.BuyerEmail,
                    ApproveFlag = request.ApproveFlag            
                };

                _context.BuyerNotes.Add(newNote);
                await _context.SaveChangesAsync();

                var seller = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.PurchaseEmail);
                var buyer = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.BuyerEmail);

                var sellerName = (seller?.FirstName + " " + seller?.LastName)?.Trim() ?? "Seller";
                var buyerName = (buyer?.FirstName + " " + buyer?.LastName)?.Trim() ?? "Buyer";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Email:User"]!),
                    Subject = $"{buyerName} wants to purchase your notes",
                    Body = $@"
                    Hello {sellerName},

                    We would like to inform you that {buyerName} wants to purchase your notes.
                    Please check the 'Buyer Requests' tab and allow download access to the buyer if you have received the payment from them.

                    Regards,
                    Notes Marketplace",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(request.PurchaseEmail!);

                using var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                {
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true
                };

                await smtpClient.SendMailAsync(mailMessage);

                return CreatedAtAction(nameof(PostBuyerNote), new { id = newNote.Id }, new
                {
                    success = true,
                    data = newNote,
                    message = "Note created and confirmation email sent."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

    }
}