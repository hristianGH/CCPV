using CCPV.Main.API.Handler;
using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/coin")]
    public class CoinController(ICoinHandler coinService, ILogger<CoinController> logger) : ControllerBase
    {
        [HttpGet("prices")]
        public async Task<IActionResult> GetPrices([FromQuery] bool forceRefresh = false)
        {
            try
            {
                //TODO make refresh time configurable
                logger.LogInformation("START: CoinController.GetPrices");

                IEnumerable<Misc.CoinPrice> prices = await coinService.GetPricesAsync(forceRefresh);
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
    }

}
