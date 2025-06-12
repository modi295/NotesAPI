using Microsoft.AspNetCore.Mvc;
using NotesAPI.Repositories;

namespace NotesAPI.Controllers
{
    [Route("api")]
    [ApiController]
    //[Authorize]
    public class SupportController : ControllerBase
    {
        private readonly ISupportRepository _supportRepository;

        public SupportController(ISupportRepository supportRepository)
        {
            _supportRepository = supportRepository;
        }

        [HttpGet("support")]
        public async Task<IActionResult> GetSupport()
        {
            try
            {
                var supportData = await _supportRepository.GetSupportAsync();

                if (supportData == null)
                {
                    return NotFound(new { error = "Support info not found" });
                }

                return Ok(supportData);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Server error" });
            }
        }
    }
}
