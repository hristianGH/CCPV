namespace CCPV.Main.API.Misc
{
    public class PortfolioEntryResponse
    {
        public string CoinSymbol { get; set; } = null!;
        public decimal BuyPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
