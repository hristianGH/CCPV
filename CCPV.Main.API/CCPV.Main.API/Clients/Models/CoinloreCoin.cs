using System.Text.Json.Serialization;

namespace CCPV.Main.API.Clients.Models
{
    public class CoinloreCoin
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price_usd")]
        public string PriceUsdRaw { get; set; } = string.Empty;

        [JsonIgnore]
        public decimal PriceUsd => decimal.TryParse(PriceUsdRaw, out decimal price) ? price : 0;
    }
}
