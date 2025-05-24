using Microsoft.AspNetCore.Mvc;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("api/1[controller]")]
    public class PortfolioController : ControllerBase
    {
 
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(ILogger<PortfolioController> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        public IEnumerable<Portfolio> Get()
        {
            return
            [
                new Portfolio { Date = DateOnly.FromDateTime(DateTime.Now), TemperatureC = 25, Summary = "Warm" },
                new Portfolio { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 30, Summary = "Hot" },
                new Portfolio { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)), TemperatureC = 20, Summary = "Cool" }
            ]
            ;
        }
    }
}
