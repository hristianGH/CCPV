using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Handler;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/1[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly ILogger<PortfolioController> _logger;
        private readonly IPortfolioHandler _portfolioHandler;

        public PortfolioController(ILogger<PortfolioController> logger, IPortfolioHandler portfolioHandler)
        {
            _logger = logger;
            _portfolioHandler = portfolioHandler;
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPortfolio([FromQuery] string portfolioName, IFormFile file)
        {
            if (string.IsNullOrEmpty(portfolioName))
                return BadRequest("Portfolio name is required.");

            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            // Try to read user id from custom header "UserId"
            if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("UserId header is missing or invalid.");
            }
            try
            {

                if (file.Length > 8 * 1024 * 1024) // 8 MB
                {
                    var jobId = await SaveAndQueueJob(file, userId);
                    return Accepted(new { JobId = jobId, Message = "File is being processed." });
                }
                else
                {
                    await _portfolioHandler.ProcessPortfolioFile(file, userId);
                    return Ok("Portfolio processed successfully.");
                }
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload portfolio");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("stream-upload")]
        public async Task<IActionResult> StreamUpload()
        {
            string? fileName = Request.Headers["X-Filename"].FirstOrDefault();
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("Missing filename header.");

            string uploadsPath = Path.Combine("Uploads", fileName);
            Directory.CreateDirectory("Uploads");

            await using FileStream fileStream = System.IO.File.Create(uploadsPath);
            await Request.Body.CopyToAsync(fileStream);

            return Accepted(new { message = "File received, processing in background" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortfolioById(Guid id)
        {
            PortfolioEntity? portfolio = await _portfolioHandler.GetPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();

            return Ok(portfolio);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPortfoliosByUserId(Guid userId)
        {
            IEnumerable<PortfolioEntity> portfolios = await _portfolioHandler.GetPortfoliosByUserIdAsync(userId);
            return Ok(portfolios);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            bool deleted = await _portfolioHandler.DeletePortfolioAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
