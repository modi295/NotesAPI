using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class SoldController : ControllerBase
    {
        private readonly NotesApplicationContext _context;
        private readonly IConfiguration _config;

        public SoldController(NotesApplicationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("soldnotes/{email}")]
        public async Task<IActionResult> GetSoldNotes(string email)
        {
            try
            {
                var soldNotes = await _context.SoldNotes
                    .Where(sn => sn.PurchaseEmail == email)
                    .ToListAsync();

                if (soldNotes == null || !soldNotes.Any())
                {
                    return NotFound(new { message = "No download notes found for this user" });
                }

                return Ok(soldNotes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching sold notes: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpPost("soldnotes")]
        public async Task<IActionResult> PostSoldNote([FromBody] SoldNote note)
        {
            try
            {
                _context.SoldNotes.Add(note);
                await _context.SaveChangesAsync();

                return StatusCode(201, new { success = true, data = note });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

    }
}