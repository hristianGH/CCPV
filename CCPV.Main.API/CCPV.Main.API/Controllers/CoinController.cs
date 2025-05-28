using CCPV.Main.API.Handler;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/coin")]
    public class CoinController(ICoinHandler coinHandler, ILogger<CoinController> logger) : ControllerBase
    {
        [HttpGet("prices")]
        public async Task<IActionResult> GetPrices(
                    [FromQuery] bool forceRefresh = false,
                    [FromQuery] int start = 0,
                    [FromQuery] int limit = 100)
        {
            try
            {
                logger.LogInformation("START: CoinController.GetPrices");
                IEnumerable<Misc.CoinPrice> prices = await coinHandler.GetPricesAsync(forceRefresh, start, limit);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                // logthe error if you have a logger injected
                logger.LogError(ex, "Failed to fetch coin prices.");
                throw;
            }
            finally
            {
                logger.LogInformation("END: CoinController.GetPrices");
            }
        }

        [HttpGet("by-symbols")]
        public async Task<IActionResult> GetPricesByIds([FromQuery] string symbols)
        {
            if (string.IsNullOrWhiteSpace(symbols))
                return BadRequest("No ids provided.");

            List<string> symbolList = [.. symbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
            IEnumerable<Misc.CoinPrice> prices = await coinHandler.GetPricesBySymbolsAsync(symbolList);
            return Ok(prices);
        }
    }

}
