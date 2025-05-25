namespace CCPV.Main.Background
{
    interface IBackgroundJob
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
