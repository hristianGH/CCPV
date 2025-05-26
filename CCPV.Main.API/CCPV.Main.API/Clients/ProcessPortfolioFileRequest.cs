namespace CCPV.Main.API.Clients
{
    public class ProcessPortfolioFileRequest
    {
        public Guid UserId { get; set; }
        public string PortfolioName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }
}
