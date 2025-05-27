using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CCPV.Main.Background.BackgroundJobs
{
    public class BackgroundJobFactory
    {
        public static void RegisterRecurringJobs(IServiceProvider serviceProvider)
        {
            IEnumerable<Type> jobTypes = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(IBackgroundJob).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (Type? jobType in jobTypes)
            {
                string jobId = $"recurring-job-{jobType.Name}";
                RecurringJob.AddOrUpdate(
                    jobId,
                    () => InvokeJob(jobType, serviceProvider),
                    GetCronExpression(jobType, serviceProvider));
            }
        }

        private static async Task InvokeJob(Type jobType, IServiceProvider provider)
        {
            using IServiceScope scope = provider.CreateScope();
            IBackgroundJob job = (IBackgroundJob)scope.ServiceProvider.GetRequiredService(jobType);
            await job.ExecuteAsync(CancellationToken.None);
        }

        private static string GetCronExpression(Type jobType, IServiceProvider provider)
        {
            using IServiceScope scope = provider.CreateScope();
            IBackgroundJob job = (IBackgroundJob)scope.ServiceProvider.GetRequiredService(jobType);
            return job.CronExpression;
        }
    }
}
