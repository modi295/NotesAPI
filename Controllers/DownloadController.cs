using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class DownloadController : Controller
    {
        private readonly NotesApplicationContext _context;
        private readonly IConfiguration _config;

        public DownloadController(NotesApplicationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("downloadnotes/{email}")]
        public async Task<IActionResult> GetDownloadNotes(string email)
        {
            try
            {
                var downloadNotes = await _context.DownloadNotes
                    .Where(dn => dn.BuyerEmail == email)
                    .Select(dn => new
                    {
                        dn.Id,
                        dn.NoteId,
                        dn.NoteTitle,
                        dn.Email,
                        dn.BuyerEmail,
                        dn.SellPrice,
                        dn.SellFor,
                        dn.Category,
                        dn.PurchaseEmail,
                        dn.CreatedAt,
                        NotesAttachment = _context.Notes
                            .Where(n => n.Id == dn.NoteId)
                            .Select(n => n.NotesAttachmentP)
                            .FirstOrDefault(),

                        BuyerFullName = _context.Users
                            .Where(u => u.Email == dn.BuyerEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),

                        BuyerPhone = _context.Users
                            .Where(u => u.Email == dn.BuyerEmail)
                            .Select(u => u.PhoneNumberCode + u.PhoneNumber)
                            .FirstOrDefault(),

                        BuyerAddress = _context.Users
                            .Where(u => u.Email == dn.BuyerEmail)
                            .Select(u => u.Address1 + ", " + u.City + ", " + u.State + ", " + u.Country + " - " + u.ZipCode)
                            .FirstOrDefault(),

                        SellerFullName = _context.Users
                            .Where(u => u.Email == dn.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),

                        SellerPhone = _context.Users
                            .Where(u => u.Email == dn.Email)
                            .Select(u => u.PhoneNumberCode + u.PhoneNumber)
                            .FirstOrDefault(),

                        SellerAddress = _context.Users
                            .Where(u => u.Email == dn.Email)
                            .Select(u => u.Address1 + ", " + u.City + ", " + u.State + ", " + u.Country + " - " + u.ZipCode)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (downloadNotes == null || !downloadNotes.Any())
                {
                    return NotFound(new { message = "No download notes found for this user" });
                }

                return Ok(downloadNotes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("downloadnotes")]
        public async Task<IActionResult> PostDownloadNote([FromBody] DownloadNote note)
        {
            try
            {
                _context.DownloadNotes.Add(note);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = note });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("downloadnotesbyId/{id}")]
        public async Task<IActionResult> GetDownloadNotesById(int id)
        {
            try
            {
                var notes = await _context.DownloadNotes
                    .Where(d => d.NoteId == id)
                    .Select(d => new
                    {
                        d.Id,
                        d.NoteId,
                        d.NoteTitle,
                        d.Category,
                        d.SellPrice,
                        d.SellFor,
                        d.Email,
                        d.BuyerEmail,
                        d.PurchaseEmail,
                        d.CreatedAt,
                        BuyerName = _context.Users
                            .Where(u => u.Email == d.BuyerEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),
                        PurchaserName = _context.Users
                            .Where(u => u.Email == d.PurchaseEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (notes == null || !notes.Any())
                    return NotFound(new { message = "No download notes found for this note ID" });

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("downloadnotes")]
        public async Task<IActionResult> GetAllDownloadNotes()
        {
            try
            {
                var notes = await _context.DownloadNotes
                    .Select(d => new
                    {
                        d.Id,
                        d.NoteId,
                        d.NoteTitle,
                        d.Category,
                        d.SellFor,
                        d.SellPrice,
                        d.PurchaseTypeFlag,
                        d.PurchaseEmail,
                        d.BuyerEmail,
                        d.Email,
                        d.CreatedAt,
                        BuyerName = _context.Users
                            .Where(u => u.Email == d.BuyerEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),

                        PurchaserName = _context.Users
                            .Where(u => u.Email == d.PurchaseEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (notes == null || notes.Count == 0)
                {
                    return NotFound(new { message = "No download notes found" });
                }

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("downloadnotesbyemail/{email}")]
        public async Task<IActionResult> GetDownloadNotesByEmail(string email)
        {
            try
            {
                var notes = await _context.DownloadNotes
                    .Where(d => d.Email == email)
                    .Select(d => new
                    {
                        d.Id,
                        d.NoteId,
                        d.NoteTitle,
                        d.Category,
                        d.SellFor,
                        d.SellPrice,
                        d.PurchaseTypeFlag,
                        d.PurchaseEmail,
                        d.BuyerEmail,
                        d.Email, // seller email
                        d.CreatedAt,
                        BuyerName = _context.Users
                            .Where(u => u.Email == d.BuyerEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),

                        PurchaserName = _context.Users
                            .Where(u => u.Email == d.PurchaseEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (notes == null || notes.Count == 0)
                {
                    return NotFound(new { message = "No download notes found for this user" });
                }

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("reviewdownloadnotesbyId/{id}")]
        public async Task<IActionResult> GetReviewFromDownloadNotesByNoteId(int id)
        {
            try
            {
                var notes = await _context.DownloadNotes
                    .Where(d => d.NoteId == id && d.Rating != null && d.Comment != null)
                    .Select(d => new
                    {
                        d.NoteId,
                        d.BuyerEmail,
                        d.Rating,
                        d.Comment,
                        BuyerName = _context.Users
                            .Where(u => u.Email == d.BuyerEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),

                        ProfilePicture = _context.Users
                            .Where(u => u.Email == d.BuyerEmail)
                            .Select(u => u.ProfilePicture)
                            .FirstOrDefault(),

                        AverageRating = Math.Round(
                            _context.DownloadNotes
                                .Where(dn => dn.NoteId == d.NoteId && dn.Rating != null)
                                .Average(dn => Convert.ToDouble(dn.Rating)), 0
                        )
                    })
                    .ToListAsync();

                if (notes == null || notes.Count == 0)
                {
                    return NotFound(new { message = "No matching notes found" });
                }

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("downloadNote/{id}")]
        public async Task<IActionResult> UpdateDownloadNote(int id, [FromBody] DownloadNote request)
        {
            try
            {
                var existingNote = await _context.DownloadNotes.FindAsync(id);
                if (existingNote == null)
                    return NotFound(new { success = false, message = "Download note not found" });

                // Update properties
                existingNote.Email = request.Email;
                existingNote.NoteId = request.NoteId;
                existingNote.NoteTitle = request.NoteTitle;
                existingNote.Category = request.Category;
                existingNote.SellFor = request.SellFor;
                existingNote.SellPrice = request.SellPrice;
                existingNote.PurchaseEmail = request.PurchaseEmail;
                existingNote.BuyerEmail = request.BuyerEmail;
                existingNote.PurchaseTypeFlag = request.PurchaseTypeFlag;
                existingNote.Rating = request.Rating;
                existingNote.Comment = request.Comment;
                existingNote.ReportRemark = request.ReportRemark;
                existingNote.UpdatedBy = request.UpdatedBy;
                existingNote.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // If report remark exists, send email
                if (!string.IsNullOrWhiteSpace(request.ReportRemark))
                {
                    var member = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.BuyerEmail);
                    var seller = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.PurchaseEmail);

                    var memberName = member != null ? $"{member.FirstName} {member.LastName}" : "Member";
                    var sellerName = seller != null ? $"{seller.FirstName} {seller.LastName}" : "Seller";

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_config["Email:User"]!),
                        Subject = $"{memberName} Reported an issue for {request.NoteTitle}",
                        Body = $@"
                    <p>Hello Admins,</p>
                    <p>We want to inform you that <strong>{memberName}</strong> reported an issue for <strong>{sellerName}</strong>'s note titled <strong>""{request.NoteTitle}""</strong>.</p>
                    <p>Please look at the note and take necessary actions.</p>
                    <p>Regards,<br/>Notes Marketplace</p>",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(_config["Email:User"]!); // Or use comma-split list if multiple admins

                    using var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                    {
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = true
                    };

                    await smtpClient.SendMailAsync(mailMessage);
                }

                return Ok(new { success = true, data = existingNote });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("reportednotes")]
        public async Task<IActionResult> GetAllReportedDownloadNotes()
        {
            try
            {
                var result = await _context.DownloadNotes
                    .Where(dn => dn.ReportRemark != null)
                    .Select(dn => new
                    {
                        dn.Id,
                        dn.Email,
                        dn.NoteId,
                        dn.NoteTitle,
                        dn.Category,
                        dn.SellFor,
                        dn.SellPrice,
                        dn.PurchaseEmail,
                        dn.BuyerEmail,
                        dn.PurchaseTypeFlag,
                        dn.Rating,
                        dn.Comment,
                        dn.ReportRemark,
                        dn.UpdatedBy,
                        dn.UpdatedAt,

                        BuyerName = _context.Users
                            .Where(u => u.Email == dn.BuyerEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),

                        PurchaserName = _context.Users
                            .Where(u => u.Email == dn.PurchaseEmail)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return NotFound(new { message = "No reported notes found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}