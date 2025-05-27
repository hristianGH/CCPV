namespace CCPV.Main.API.Misc
{
    public class ProcessPortfolioFileRequest
    {
        public Guid UserId { get; set; }
        public string PortfolioName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public override string ToString()
        {
            return $"UserId: {UserId}, PortfolioName: {PortfolioName}, FilePath: {FilePath}";
        }
    }
}
