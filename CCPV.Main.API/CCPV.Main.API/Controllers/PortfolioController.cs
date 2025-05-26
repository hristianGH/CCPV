using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Handler;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/1[controller]")]
    public class PortfolioController(ILogger<PortfolioController> logger, IPortfolioHandler portfolioHandler, IUploadHandler uploadHandler) : ControllerBase
    {
        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadPortfolio([FromQuery] string portfolioName, IFormFile file)
        //{
        //    if (string.IsNullOrEmpty(portfolioName))
        //        return BadRequest("Portfolio name is required.");

        //    if (file == null || file.Length == 0)
        //        return BadRequest("File is required.");
        //    if (file.Length > 8 * 1024 * 1024)

        //    {
        //        return BadRequest("File size exceeds the maximum limit of 8MB. Please use api/upload instead");
        //    }
        //    if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        //    {
        //        return BadRequest("UserId header is missing or invalid.");
        //    }
        //    //try
        //    //{
        //    //    uploadHandler.lightweightUpload
        //    //}
        //    //catch (Exception)
        //    //{
        //    //    logger.LogError(ex, "Failed to upload portfolio");
        //    //    throw;
        //    //}
        //    //try
        //    //{
        //    //    await portfolioHandler.ProcessPortfolioAsync(portfolioName, file);
        //    //    return Ok("Portfolio processed successfully.");
        //    //}
        //    //catch (FormatException ex)
        //    //{
        //    //    return BadRequest(ex.Message);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    logger.LogError(ex, "Failed to processs portfolio");
        //    //    throw;
        //    //}
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortfolioById(Guid id)
        {
            PortfolioEntity? portfolio = await portfolioHandler.GetNoTrackingPortfolioByIdAsync(id);
            if (portfolio == null) return NotFound();

            return Ok(portfolio);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPortfoliosByUserId(Guid userId)
        {
            IEnumerable<PortfolioEntity> portfolios = await portfolioHandler.GetPortfoliosByUserIdAsync(userId);
            return Ok(portfolios);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            bool deleted = await portfolioHandler.DeletePortfolioAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
