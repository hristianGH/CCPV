using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        [ApiController]
        [Route("api/coin")]
        public class CoinController(ICoinHandler coinService) : ControllerBase
        {
            [HttpGet("prices")]
            public async Task<IActionResult> GetPrices([FromQuery] bool forceRefresh = false)
            {
                try
                {
                    var prices = await coinService.GetPricesAsync(forceRefresh);
                    return Ok(prices);
                }
                catch (Exception ex)
                {
                    // Log error
                    return StatusCode(500, "Failed to retrieve coin prices.");
                }
            }
        }

    }
}
