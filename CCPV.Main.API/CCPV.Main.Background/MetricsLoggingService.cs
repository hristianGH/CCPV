using Microsoft.Extensions.Logging;

namespace CCPV.Main.Background
{
    public class MetricsLoggingService : IBackgroundJob
    {
        private readonly ILogger<MetricsLoggingService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MetricsLoggingService(ILogger<MetricsLoggingService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

                    var userCount = await db.Users.CountAsync(stoppingToken);
                    var portfolioCount = await db.Portfolios.CountAsync(stoppingToken);

                    _logger.LogInformation("Metrics: Users={UserCount}, Portfolios={PortfolioCount}", userCount, portfolioCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log metrics.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Run every 30 seconds
            }
        }
    }
}
