using CCPV.Main.API.Handler;
using CCPV.Main.Background.BackgroundJobs;

namespace CCPV.Main.API.BackgroundJobs
{
    public class CoinSyncBackgroundJob(ILogger<CoinSyncBackgroundJob> logger, ICoinHandler coinHandler) : IBackgroundJob
    {
        // Every day at midnight
        public string CronExpression => "0 0 0 * * *";

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("START: CoinSyncBackgroundJob.ExecuteAsync");

            try
            {
                await coinHandler.GetAllCoinsMetadataAsync(forceRefresh: true);
                logger.LogInformation("Successfully refreshed and cached all coin metadata.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while syncing coin metadata.");
                throw;
            }
            finally
            {
                logger.LogInformation("END: CoinSyncBackgroundJob.ExecuteAsync");
            }
        }
    }

}
