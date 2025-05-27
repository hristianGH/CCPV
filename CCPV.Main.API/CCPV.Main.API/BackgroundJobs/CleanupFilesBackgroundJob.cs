using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Misc.Enums;
using CCPV.Main.Background.BackgroundJobs;
using Hangfire;

namespace CCPV.Main.API.BackgroundJobs
{
    public class CleanupFilesBackgroundJob(ILogger<CleanupFilesBackgroundJob> logger, IServiceProvider serviceProvider) : IBackgroundJob
    {
        // every 15 minutes
        public string CronExpression => "*/15 * * * *";

        [DisableConcurrentExecution(timeoutInSeconds: 900)]
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Starting upload cleanup job");
                using IServiceScope scope = serviceProvider.CreateScope();
                ApiDbContext db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

                DateTime threshold = DateTime.UtcNow.AddHours(-1);

                List<UploadStatusEntity> expiredUploads = [.. db.UploadStatuses
                .Where(x =>
                    x.Status != UploadStatusEnum.Deleted.ToString() &&
                    x.LastUpdated < threshold)];

                int deletedCount = 0;

                foreach (UploadStatusEntity upload in expiredUploads)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(upload.FilePath) && Directory.Exists(upload.FilePath))
                        {
                            Directory.Delete(upload.FilePath, true);
                            logger.LogInformation("Deleted upload folder: {Path}", upload.FilePath);
                        }

                        upload.Status = UploadStatusEnum.Deleted.ToString();
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to delete upload folder: {Path}", upload.FilePath);
                    }
                }

                await db.SaveChangesAsync();

                logger.LogInformation("Upload cleanup complete. Marked deleted: {Count}", deletedCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during upload cleanup job");
                throw;
            }
        }
    }
}