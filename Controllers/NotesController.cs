using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;
using System.Net.Mail;

namespace NotesAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class NotesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly NotesApplicationContext _context;
        private readonly IConfiguration _config;

        public NotesController(IWebHostEnvironment env, NotesApplicationContext context, IConfiguration config)
        {
            _env = env;
            _context = context;
            _config = config;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadNote([FromForm] IFormCollection form)
        {
            try
            {
                var email = form["email"];
                var noteTitle = form["noteTitle"];
                var category = form["category"];
                var notesType = form["notesType"];
                int numberOfPages = int.TryParse(form["numberOfPages"], out var parsedPages) ? parsedPages : 0;
                var statusFlag = form["statusFlag"];
                var publishFlag = form["publishFlag"];

                // Save uploaded files
                var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsDirImages = Path.Combine(wwwRoot, "uploads", "images");
                var uploadsDirPdfs = Path.Combine(wwwRoot, "uploads", "pdfs");

                Directory.CreateDirectory(uploadsDirImages);
                Directory.CreateDirectory(uploadsDirPdfs);

                string? displayPicturePath = null;
                string? previewPath = null;
                List<string> attachmentPaths = new();

                if (form.Files["displayPictureP"] != null)
                {
                    var file = form.Files["displayPictureP"];
                    var filePath = Path.Combine(uploadsDirImages, file!.FileName!);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    displayPicturePath = $"http://localhost:5000/uploads/images/{file.FileName}";
                }

                if (form.Files["previewUploadP"] != null)
                {
                    var file = form.Files["previewUploadP"];
                    var filePath = Path.Combine(uploadsDirPdfs, file!.FileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    previewPath = $"http://localhost:5000/uploads/pdfs/{file.FileName}";
                }

                if (form.Files.GetFiles("notesAttachmentP") is var attachments && attachments.Count > 0)
                {
                    foreach (var file in attachments)
                    {
                        var filePath = Path.Combine(uploadsDirPdfs, file.FileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);
                        attachmentPaths.Add($"http://localhost:5000/uploads/pdfs/{file.FileName}");
                    }
                }

                var note = new Note
                {
                    Email = email!,
                    NoteTitle = noteTitle!,
                    Category = category!,
                    NotesType = notesType!,
                    NumberOfPages = numberOfPages,
                    NotesDescription = form["notesDescription"]!.ToString(),
                    UniversityInformation = form["universityInformation"],
                    Country = form["country"]!.ToString(),
                    CourseInformation = form["courseInformation"],
                    CourseCode = form["courseCode"],
                    ProfessorLecturer = form["professorLecturer"],
                    SellFor = form["sellFor"]!.ToString(),
                    SellPrice = decimal.TryParse(form["sellPrice"], out var price) ? (double)price : 0,
                    DisplayPictureP = displayPicturePath,
                    PreviewUploadP = previewPath,
                    NotesAttachmentP = string.Join(",", attachmentPaths),
                    StatusFlag = statusFlag,
                    PublishFlag = publishFlag,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AddedBy = email
                };

                _context.Notes.Add(note);
                await _context.SaveChangesAsync();

                // Send email to admin
                if (statusFlag == "P")
                {
                    var user = _context.Users.FirstOrDefault(x => x.Email == email);
                    var fullName = $"{user?.FirstName} {user?.LastName}".Trim();

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_config["Email:User"]!),
                        Subject = "Note Marketplace - Note Submitted for Review",
                        Body = $@"
                            <p>Hello Admins,</p>
                            <p>We want to inform you that <strong>{fullName}</strong> sent his note titled <strong>""{noteTitle}""</strong> for review.</p>
                            <p>Please look at the note and take necessary actions.</p>
                            <p>Regards,<br/>Notes Marketplace</p>",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(_config["Email:User"]!);

                    using var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                    {
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = true
                    };

                    await smtpClient.SendMailAsync(mailMessage);
                }

                return CreatedAtAction(nameof(UploadNote), new { id = note.Id }, note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to upload note", details = ex.Message });
            }
        }

        [HttpPut("updateNotes/{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromForm] IFormCollection form)
        {
            try
            {
                var note = await _context.Notes.FindAsync(id);
                if (note == null)
                    return NotFound(new { error = "Note not found" });

                // Extract fields
                var email = form["email"];
                note.Email = email!;
                note.NoteTitle = form["noteTitle"]!;
                note.Category = form["category"]!;
                note.NotesType = form["notesType"]!;
                note.NotesDescription = form["notesDescription"]!;
                note.UniversityInformation = form["universityInformation"];
                note.Country = form["country"]!;
                note.CourseInformation = form["courseInformation"];
                note.CourseCode = form["courseCode"];
                note.ProfessorLecturer = form["professorLecturer"];
                note.SellFor = form["sellFor"]!;
                note.SellPrice = decimal.TryParse(form["sellPrice"], out var price) ? (double)price : note.SellPrice;
                note.StatusFlag = form["statusFlag"];
                note.PublishFlag = form["publishFlag"];
                note.Remark = form["remark"];
                note.UpdatedBy = email;

                if (int.TryParse(form["numberOfPages"], out var pages))
                    note.NumberOfPages = pages;

                var wwwRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsDirImages = Path.Combine(wwwRoot, "uploads", "images");
                var uploadsDirPdfs = Path.Combine(wwwRoot, "uploads", "pdfs");

                Directory.CreateDirectory(uploadsDirImages);
                Directory.CreateDirectory(uploadsDirPdfs);

                // Handle image upload
                if (form.Files["displayPictureP"] is IFormFile dp)
                {
                    var path = Path.Combine(uploadsDirImages, dp.FileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await dp.CopyToAsync(stream);
                    note.DisplayPictureP = $"http://localhost:5000/uploads/images/{dp.FileName}";
                }
                else if (!string.IsNullOrEmpty(form["displayPictureP"]) && form["displayPictureP"] != "[object FileList]")
                {
                    note.DisplayPictureP = form["displayPictureP"];
                }

                // Handle notesAttachmentP
                if (form.Files.GetFiles("notesAttachmentP") is var files && files.Count > 0)
                {
                    List<string> attachments = new();
                    foreach (var file in files)
                    {
                        var path = Path.Combine(uploadsDirPdfs, file.FileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await file.CopyToAsync(stream);
                        attachments.Add($"http://localhost:5000/uploads/pdfs/{file.FileName}");
                    }
                    note.NotesAttachmentP = string.Join(",", attachments);
                }
                else if (!string.IsNullOrEmpty(form["notesAttachmentP"]))
                {
                    note.NotesAttachmentP = form["notesAttachmentP"];
                }

                // Handle preview upload
                if (form.Files["previewUploadP"] is IFormFile pu)
                {
                    var path = Path.Combine(uploadsDirPdfs, pu.FileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await pu.CopyToAsync(stream);
                    note.PreviewUploadP = $"http://localhost:5000/uploads/pdfs/{pu.FileName}";
                }
                else if (!string.IsNullOrEmpty(form["previewUploadP"]) && form["previewUploadP"] != "[object FileList]")
                {
                    note.PreviewUploadP = form["previewUploadP"];
                }

                note.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Email Notification
                if (note.StatusFlag == "P")
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    var fullName = $"{user?.FirstName} {user?.LastName}".Trim();

                    var mail = new MailMessage
                    {
                        From = new MailAddress(_config["Email:User"]!),
                        Subject = $"{fullName} sent a note for review",
                        Body = $"""
                        Hello Admins,

                        We want to inform you that {fullName} sent his note "{note.NoteTitle}" for review. 
                        Please look at the note and take required actions.

                        Regards,
                        Notes Marketplace
                        """,
                        IsBodyHtml = false
                    };
                    mail.To.Add(_config["Email:User"]!);

                    using var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                    {
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = true
                    };

                    await smtpClient.SendMailAsync(mail);
                }

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("allpublishNotes")]
        public async Task<IActionResult> GetAllPublishedNotes()
        {
            try
            {
                // Step 1: Fetch published notes
                var publishedNotes = await _context.Notes
                    .Where(n => n.PublishFlag == "P")
                    .ToListAsync();

                if (!publishedNotes.Any())
                {
                    return NotFound(new { message = "No published notes found" });
                }

                // Step 2: Fetch downloads and users
                var downloads = await _context.DownloadNotes.ToListAsync();
                var users = await _context.Users.ToListAsync();

                // Step 3: Compose result in memory
                var result = publishedNotes.Select(n =>
                {
                    var noteDownloads = downloads.Where(d => d.NoteId == n.Id).ToList();

                    var avgRating = noteDownloads
                        .Where(d => !string.IsNullOrEmpty(d.Rating) && double.TryParse(d.Rating, out _))
                        .Select(d => double.Parse(d.Rating!))
                        .DefaultIfEmpty(0)
                        .Average();

                    var user = users.FirstOrDefault(u => u.Email == n.Email);
                    var userFullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

                    return new
                    {
                        n.Id,
                        n.NoteTitle,
                        n.Category,
                        n.NotesType,
                        n.NumberOfPages,
                        n.SellFor,
                        n.SellPrice,
                        n.DisplayPictureP,
                        n.NotesAttachmentP,
                        n.PreviewUploadP,
                        n.Country,
                        n.UniversityInformation,
                        n.CourseInformation,
                        n.CourseCode,
                        n.ProfessorLecturer,
                        n.StatusFlag,
                        n.PublishFlag,
                        n.CreatedAt,
                        n.UpdatedAt,
                        DownloadCount = noteDownloads.Count,
                        UserFullName = userFullName,
                        AverageRating = Math.Round(avgRating, 1),
                        ReportCount = noteDownloads
                            .Where(d => !string.IsNullOrEmpty(d.ReportRemark))
                            .Select(d => d.Email)
                            .Distinct()
                            .Count()
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("publishNotes/{email}")]
        public async Task<IActionResult> GetPublishedNotesByEmail(string email)
        {
            try
            {
                var notes = await _context.Notes
                    .Where(n => n.Email == email && n.PublishFlag == "P")
                    .ToListAsync();

                if (!notes.Any())
                    return NotFound(new { message = "No published notes found for this user" });

                var result = notes.Select(note =>
                {
                    var downloads = _context.DownloadNotes.Where(d => d.NoteId == note.Id).ToList();

                    var avgRating = downloads
                        .Where(d => !string.IsNullOrEmpty(d.Rating) && double.TryParse(d.Rating, out _))
                        .Select(d => double.Parse(d.Rating!))
                        .DefaultIfEmpty(0)
                        .Average();

                    return new
                    {
                        note.Id,
                        note.NoteTitle,
                        note.Category,
                        note.NotesType,
                        note.NumberOfPages,
                        note.SellFor,
                        note.SellPrice,
                        note.DisplayPictureP,
                        note.NotesAttachmentP,
                        note.PreviewUploadP,
                        note.Country,
                        note.UniversityInformation,
                        note.CourseInformation,
                        note.CourseCode,
                        note.ProfessorLecturer,
                        note.StatusFlag,
                        note.PublishFlag,
                        note.CreatedAt,
                        note.UpdatedAt,

                        DownloadCount = downloads.Count,
                        UserFullName = _context.Users
                            .Where(u => u.Email == note.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault() ?? "",

                        AverageRating = Math.Round(avgRating, 1),
                        ReportCount = downloads
                            .Where(d => !string.IsNullOrEmpty(d.ReportRemark))
                            .Select(d => d.Email)
                            .Distinct()
                            .Count()
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("saveNotes/{email}")]
        public async Task<IActionResult> GetSavedNotesByEmail(string email)
        {
            try
            {
                var savedNotes = await _context.Notes
                    .Where(n => n.Email == email && n.StatusFlag == "S")
                    .ToListAsync();

                if (!savedNotes.Any())
                {
                    return NotFound(new { message = "No saved notes found for this user" });
                }

                var result = savedNotes.Select(note => new
                {
                    note.Id,
                    note.NoteTitle,
                    note.Category,
                    note.NotesType,
                    note.NumberOfPages,
                    note.SellFor,
                    note.SellPrice,
                    note.DisplayPictureP,
                    note.NotesAttachmentP,
                    note.PreviewUploadP,
                    note.Country,
                    note.UniversityInformation,
                    note.CourseInformation,
                    note.CourseCode,
                    note.ProfessorLecturer,
                    note.StatusFlag,
                    note.PublishFlag,
                    note.CreatedAt,
                    note.UpdatedAt
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("getDashboardData/{email}")]
        public async Task<IActionResult> GetNotesDashboardData(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                var soldNotesCount = await _context.SoldNotes
                    .CountAsync(s => s.PurchaseEmail == email);

                var downloadedNotesCount = await _context.DownloadNotes
                    .CountAsync(d => d.BuyerEmail == email);

                var soldNotesTotalPrice = await _context.SoldNotes
                    .Where(s => s.PurchaseEmail == email)
                    .SumAsync(s => (double?)s.SellPrice) ?? 0;

                var rejectedNotesCount = await _context.Notes
                    .CountAsync(n => n.Email == email && n.PublishFlag == "R");

                var buyerNotesCount = await _context.BuyerNotes
                    .CountAsync(b => b.PurchaseEmail == email);

                return Ok(new
                {
                    soldNotesCount,
                    downloadedNotesCount,
                    soldNotesTotalPrice,
                    rejectedNotesCount,
                    buyerNotesCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("getAdminDashboardData")]
        public async Task<IActionResult> GetAdminDashboardData()
        {
            try
            {
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

                var inReviewNotesCount = await _context.Notes
                    .CountAsync(n => n.PublishFlag == "I");

                var newDownloadsCount = await _context.DownloadNotes
                    .CountAsync(dn => dn.CreatedAt >= sevenDaysAgo);

                var newRegistrationsCount = await _context.Users
                    .CountAsync(u =>
                        u.CreatedAt >= sevenDaysAgo &&
                        (u.Active == null || u.Active == "Y"));

                return Ok(new
                {
                    inReviewNotesCount,
                    newDownloadsCount,
                    newRegistrationsCount
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching dashboard metrics: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("notesById/{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            try
            {
                var note = await _context.Notes
                    .Where(n => n.Id == id)
                    .Select(n => new
                    {
                        n.Id,
                        n.NoteTitle,
                        n.Category,
                        n.NotesType,
                        n.NumberOfPages,
                        n.SellFor,
                        n.SellPrice,
                        n.DisplayPictureP,
                        n.NotesAttachmentP,
                        n.PreviewUploadP,
                        n.Country,
                        n.UniversityInformation,
                        n.CourseInformation,
                        n.CourseCode,
                        n.ProfessorLecturer,
                        n.StatusFlag,
                        n.PublishFlag,
                        n.CreatedAt,
                        n.UpdatedAt,
                        ReportCount = _context.DownloadNotes
                            .Where(dn => dn.NoteId == n.Id && dn.ReportRemark != null)
                            .Select(dn => dn.Email)
                            .Distinct()
                            .Count()
                    })
                    .FirstOrDefaultAsync();

                if (note == null)
                {
                    return NotFound(new { message = "Note not found" });
                }

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("deleteNote/{id}")]
        public async Task<IActionResult> DeleteNoteById(int id)
        {
            try
            {
                var note = await _context.Notes.FindAsync(id);
                if (note == null)
                {
                    return NotFound(new { message = "Note not found" });
                }

                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Note deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("allNotes")]
        public async Task<IActionResult> GetAllNotes()
        {
            try
            {
                var allNotes = await _context.Notes.ToListAsync();

                if (allNotes == null || !allNotes.Any())
                {
                    return NotFound(new { message = "No notes found" });
                }

                return Ok(allNotes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("underReviewNotes")]
        public async Task<IActionResult> GetUnderReviewNotes()
        {
            try
            {
                var underReviewNotes = await _context.Notes
                    .Where(n => n.StatusFlag == "P" && n.PublishFlag != "R" && n.PublishFlag != "P")
                    .Select(n => new
                    {
                        n.Id,
                        n.NoteTitle,
                        n.Category,
                        n.NotesType,
                        n.SellFor,
                        n.SellPrice,
                        n.DisplayPictureP,
                        n.CreatedAt,
                        n.UpdatedAt,
                        n.StatusFlag,
                        n.PublishFlag,
                        UserFullName = _context.Users
                            .Where(u => u.Email == n.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!underReviewNotes.Any())
                {
                    return NotFound(new { message = "No published notes found" });
                }

                return Ok(underReviewNotes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error fetching published notes: " + ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("rejectedNotes")]
        public async Task<IActionResult> GetAllRejectedNotes()
        {
            try
            {
                var rejectedNotes = await _context.Notes
                    .Where(n => n.PublishFlag == "R")
                    .Select(n => new
                    {
                        n.Id,
                        n.NoteTitle,
                        n.Category,
                        n.NotesType,
                        n.NumberOfPages,
                        n.SellFor,
                        n.SellPrice,
                        n.DisplayPictureP,
                        n.NotesAttachmentP,
                        n.PreviewUploadP,
                        n.Country,
                        n.UniversityInformation,
                        n.StatusFlag,
                        n.PublishFlag,
                        n.CreatedAt,
                        n.UpdatedAt,
                        DownloadCount = _context.DownloadNotes.Count(d => d.NoteId == n.Id),
                        UserFullName = _context.Users
                            .Where(u => u.Email == n.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!rejectedNotes.Any())
                {
                    return NotFound(new { message = "No rejected notes found" });
                }

                return Ok(rejectedNotes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching rejected notes: " + ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("underReviewNotes/{email}")]
        public async Task<IActionResult> GetUnderReviewNotesByEmail(string email)
        {
            try
            {
                var notes = await _context.Notes
                    .Where(n =>
                        n.Email == email &&
                        n.StatusFlag == "P" &&
                        n.PublishFlag != "R" &&
                        n.PublishFlag != "P")
                    .Select(n => new
                    {
                        n.Id,
                        n.NoteTitle,
                        n.Category,
                        n.NotesType,
                        n.NumberOfPages,
                        n.SellFor,
                        n.SellPrice,
                        n.DisplayPictureP,
                        n.NotesAttachmentP,
                        n.PreviewUploadP,
                        n.Country,
                        n.UniversityInformation,
                        n.StatusFlag,
                        n.PublishFlag,
                        n.CreatedAt,
                        n.UpdatedAt,
                        UserFullName = _context.Users
                            .Where(u => u.Email == n.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!notes.Any())
                {
                    return NotFound(new { message = "No under review notes found for the given email" });
                }

                return Ok(notes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching under review notes: " + ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("getUserNotesByEmail/{email}")]
        public async Task<IActionResult> GetUserNotesByEmail(string email)
        {
            try
            {
                var notes = await _context.Notes
                    .Where(n => n.Email == email && n.StatusFlag == "P")
                    .Select(n => new
                    {
                        Note = n,
                        DownloadCount = _context.DownloadNotes.Count(d => d.NoteId == n.Id),
                        UserFullName = _context.Users
                            .Where(u => u.Email == n.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!notes.Any())
                {
                    return NotFound(new { message = "No published notes found for the given email" });
                }

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("rejectedNotesByEmail/{email}")]
        public async Task<IActionResult> GetAllRejectedNotesByEmail(string email)
        {
            try
            {
                var notes = await _context.Notes
                    .Where(n => n.Email == email && n.PublishFlag == "R")
                    .Select(n => new
                    {
                        Note = n,
                        DownloadCount = _context.DownloadNotes.Count(d => d.NoteId == n.Id),
                        UserFullName = _context.Users
                            .Where(u => u.Email == n.Email)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!notes.Any())
                {
                    return NotFound(new { message = "No rejected notes found for this user" });
                }

                return Ok(notes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching rejected notes: " + ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("updateNotesStatus/{id}")]
        public async Task<IActionResult> UpdateNoteStatus(int id, [FromBody] Note updatedData)
        {
            try
            {
                var existingNote = await _context.Notes.FindAsync(id);
                if (existingNote == null)
                    return NotFound(new { success = false, error = "Note not found" });

                existingNote.Email = updatedData.Email;
                existingNote.NoteTitle = updatedData.NoteTitle;
                existingNote.Category = updatedData.Category;
                existingNote.NotesType = updatedData.NotesType;
                existingNote.NumberOfPages = updatedData.NumberOfPages;
                existingNote.NotesDescription = updatedData.NotesDescription;
                existingNote.UniversityInformation = updatedData.UniversityInformation;
                existingNote.Country = updatedData.Country;
                existingNote.CourseInformation = updatedData.CourseInformation;
                existingNote.CourseCode = updatedData.CourseCode;
                existingNote.ProfessorLecturer = updatedData.ProfessorLecturer;
                existingNote.SellFor = updatedData.SellFor;
                existingNote.SellPrice = updatedData.SellPrice;
                existingNote.StatusFlag = updatedData.StatusFlag;
                existingNote.PublishFlag = updatedData.PublishFlag;
                existingNote.Remark = updatedData.Remark;
                existingNote.DisplayPictureP = updatedData.DisplayPictureP ?? existingNote.DisplayPictureP;
                existingNote.NotesAttachmentP = updatedData.NotesAttachmentP ?? existingNote.NotesAttachmentP;
                existingNote.PreviewUploadP = updatedData.PreviewUploadP ?? existingNote.PreviewUploadP;
                existingNote.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == updatedData.Email);
                var sellerName = $"{user?.FirstName} {user?.LastName}".Trim();

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["Email:User"]!),
                    To = { updatedData.Email },
                    Subject = "Sorry! We need to remove your notes from our portal.",
                    Body = $@"
                Hello {sellerName},

                We want to inform you that your note '{updatedData.NoteTitle}' has been removed from the portal.

                Please find our remarks below:
                {updatedData.Remark}

                Regards,
                Notes Marketplace",
                    IsBodyHtml = false
                };

                using var smtpClient = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
                {
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true
                };

                await smtpClient.SendMailAsync(mailMessage);

                return Ok(new { success = true, data = existingNote });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

    }
}
