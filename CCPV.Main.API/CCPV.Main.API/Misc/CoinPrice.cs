namespace CCPV.Main.API.Misc
{
    public class CoinPrice
    {
        public string Id { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal PriceUsd { get; set; }
    }
}
