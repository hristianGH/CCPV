using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Handler;
using CCPV.Main.API.Misc;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController(ILogger<PortfolioController> logger, IPortfolioHandler portfolioHandler, IUploadHandler uploadHandler) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPortfolio([FromQuery] string portfolioName, IFormFile file)
        {
            logger.LogInformation($"START: PortfolioController.UploadPortfolio portfolioName: {portfolioName}");
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
                logger.LogError(ex, $"ERROR. PortfolioController.UploadPortfolio portfolioName: {portfolioName}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioController.UploadPortfolio portfolioName: {portfolioName}");
            }
        }

        [HttpPost("process-from-file")]
        public async Task<IActionResult> ProcessFromFile([FromBody] ProcessPortfolioFileRequest request)
        {
            try
            {
                logger.LogInformation($"START: PortfolioController.ProcessFromFile {request.ToString()}");
                if (string.IsNullOrEmpty(request.PortfolioName) || string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest("Portfolio name and file path are required.");
                }
                PortfolioEntity portfolio = await portfolioHandler.UploadPortfolioFromPathAsync(request.UserId, request.PortfolioName, request.FilePath);
                // TO DO make a class response instead of anonymous object
                return Ok(new
                {
                    message = "Portfolio processing started.",
                    portfolioId = portfolio.Id
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioController.ProcessFromFile {request.ToString()}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioController.ProcessFromFile {request.ToString()}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortfolioById(Guid portfolioId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioController.GetPortfolioById portfolioId: {portfolioId}");

                if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return BadRequest("UserId header is missing or invalid.");
                }
                PortfolioEntity? portfolio = await portfolioHandler.GetNoTrackingPortfolioByIdAsync(portfolioId, userId);
                if (portfolio == null) return NotFound();
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioController.GetPortfolioById portfolioId: {portfolioId}"); ;
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioController.GetPortfolioById portfolioId: {portfolioId}");
            }
        }

        [HttpGet()]
        public async Task<IActionResult> GetPortfoliosByUserId()
        {
            try
            {
                logger.LogInformation("START: PortfolioController.GetPortfoliosByUserId");
                if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return BadRequest("UserId header is missing or invalid.");
                }
                IEnumerable<PortfolioEntity> portfolios = await portfolioHandler.GetPortfoliosByUserIdAsync(userId);
                return Ok(portfolios);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR: PortfolioController.GetPortfoliosByUserId");

                throw;
            }
            finally
            {
                logger.LogInformation("END: PortfolioController.GetPortfoliosByUserId");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(Guid portfolioId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioController.DeletePortfolio portfolioId: {portfolioId}");
                if (!Request.Headers.TryGetValue("UserId", out Microsoft.Extensions.Primitives.StringValues userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return BadRequest("UserId header is missing or invalid.");
                }
                await portfolioHandler.DeletePortfolioAsync(userId, portfolioId);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioController.DeletePortfolio portfolioId: {portfolioId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioController.DeletePortfolio portfolioId: {portfolioId}");
            }
        }
    }
}
