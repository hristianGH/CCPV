namespace CCPV.Main.API.Misc
{
    public class PortfolioResponse
    {
        public string Name { get; set; } = null!;
        public List<PortfolioEntryResponse> Coins { get; set; } = [];
    }
}
