namespace CCPV.Main.Background
{
    interface IBackgroundJob
    {
        string CronExpression { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
