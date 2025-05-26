using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Handler;
using CCPV.Main.API.Misc;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/1[controller]")]
    public class PortfolioController(ILogger<PortfolioController> logger, IPortfolioHandler portfolioHandler, IUploadHandler uploadHandler) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPortfolio([FromQuery] string portfolioName, IFormFile file)
        {
            if (string.IsNullOrEmpty(portfolioName))
                return BadRequest("Portfolio name is required.");

            if (file == null || file.Length == 0)
                return BadRequest("File is required.");
            if (file.Length > 8 * 1024 * 1024)

            {
                return BadRequest("File size exceeds the maximum limit of 8MB. Please use api/upload instead");
            }
            if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("UserId header is missing or invalid.");
            }
            try
            {
                await portfolioHandler.UploadPortfolioAsync(userId, portfolioName, file);

                return Ok(new
                {
                    message = "Portfolio uploaded successfully.",
                    portfolioName = portfolioName
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to processs portfolio");
                throw;
            }
        }

        [HttpPost("process-from-file")]
        public async Task<IActionResult> ProcessFromFile([FromBody] ProcessPortfolioFileRequest request)
        {
            if (!System.IO.File.Exists(request.FilePath))
                return BadRequest("File does not exist.");

            try
            {
                // In real-world, you'd likely queue this
                PortfolioEntity portfolio = await portfolioHandler.UploadPortfolioFromPathAsync(request.UserId, request.PortfolioName, request.FilePath);
                return Ok(new
                {
                    message = "Portfolio processing started.",
                    portfolioId = portfolio.Id
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process portfolio from file.");
                return StatusCode(500, "Internal error during portfolio processing.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortfolioById(Guid id)
        {
            if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("UserId header is missing or invalid.");
            }
            PortfolioEntity? portfolio = await portfolioHandler.GetNoTrackingPortfolioByIdAsync(id, userId);
            if (portfolio == null) return NotFound();
            return Ok(portfolio);
        }

        [HttpGet()]
        public async Task<IActionResult> GetPortfoliosByUserId()
        {
            if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("UserId header is missing or invalid.");
            }
            IEnumerable<PortfolioEntity> portfolios = await portfolioHandler.GetPortfoliosByUserIdAsync(userId);
            return Ok(portfolios);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("UserId header is missing or invalid.");
            }
            await portfolioHandler.DeletePortfolioAsync(userId, id);
            return NoContent();
        }
    }
}
