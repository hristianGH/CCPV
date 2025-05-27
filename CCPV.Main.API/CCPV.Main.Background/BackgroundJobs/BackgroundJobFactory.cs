using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace CCPV.Main.Background.BackgroundJobs
{
    public class BackgroundJobFactory
    {
        public static void RegisterRecurringJobs(IServiceProvider serviceProvider)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            IEnumerable<IBackgroundJob> jobs = scope.ServiceProvider.GetServices<IBackgroundJob>();

            foreach (IBackgroundJob job in jobs)
            {
                string jobId = $"recurring-job-{job.GetType}";
                RecurringJob.AddOrUpdate(
                    jobId,
                    () => job.ExecuteAsync(CancellationToken.None), job.CronExpression);
            }
        }
    }
}
