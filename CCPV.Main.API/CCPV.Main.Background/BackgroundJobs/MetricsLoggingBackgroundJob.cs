using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CCPV.Main.Background.BackgroundJobs
{
    public class MetricsLoggingBackgroundJob : IBackgroundJob
    {
        private readonly ILogger<MetricsLoggingBackgroundJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MetricsLoggingBackgroundJob(ILogger<MetricsLoggingBackgroundJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        // every 15 minutes
        public string CronExpression => "*/15 * * * *";

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = _serviceProvider.CreateScope();

                    //var userCount = await db.Users.CountAsync(stoppingToken);
                    //var portfolioCount = await db.Portfolios.CountAsync(stoppingToken);

                    //_logger.LogInformation("Metrics: Users={UserCount}, Portfolios={PortfolioCount}", userCount, portfolioCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log metrics.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
