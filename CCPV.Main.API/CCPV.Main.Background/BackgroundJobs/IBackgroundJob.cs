namespace CCPV.Main.Background.BackgroundJobs
{
    public interface IBackgroundJob
    {
        string CronExpression { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
