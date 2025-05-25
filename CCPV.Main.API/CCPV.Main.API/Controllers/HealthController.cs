using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CCPV.Main.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private static readonly Stopwatch Uptime = Stopwatch.StartNew();

        [HttpGet]
        public IActionResult GetBasicHealth()
        {
            // Simple check: always return 200 OK
            return Ok(new
            {
                status = "Healthy",
                uptime = Uptime.Elapsed.ToString(@"dd\.hh\:mm\:ss")
            });
        }

        [HttpGet("stats")]
        public IActionResult GetDetailedHealth()
        {
            var healthStats = new
            {
                status = "Healthy",
                uptime = Uptime.Elapsed.ToString(@"dd\.hh\:mm\:ss"),
                timestamp = DateTime.UtcNow,
                // Example placeholders:
                databaseConnected = true,
                cacheAvailable = true,
                recentErrors = 0
            };

            return Ok(healthStats);
        }
    }
}
