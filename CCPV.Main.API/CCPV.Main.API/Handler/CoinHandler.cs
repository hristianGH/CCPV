using CCPV.Main.API.Misc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CCPV.Main.API.Handler
{
    public class CoinHandler(HttpClient httpClient, IMemoryCache cache, ILogger<CoinHandler> logger) : ICoinHandler
    {
        private const string CoinPricesCacheKey = "CoinPrices";
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public async Task<IEnumerable<CoinPrice>> GetPricesAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && cache.TryGetValue(CoinPricesCacheKey, out IEnumerable<CoinPrice> cached))
            {
                return cached;
            }
            // Fetch from Coinlore API
            HttpResponseMessage response = await httpClient.GetAsync("https://api.coinlore.net/api/tickers/");
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using JsonDocument doc = await JsonDocument.ParseAsync(stream);

            List<CoinPrice> prices = doc.RootElement
                            .GetProperty("data")
                            .EnumerateArray()
                            .Select(e => new CoinPrice
                            {
                                Symbol = e.GetProperty("symbol").GetString()!,
                                Name = e.GetProperty("name").GetString()!,
                                PriceUsd = decimal.Parse(e.GetProperty("price_usd").GetString()!)
                            })
                            .ToList();

            cache.Set(CoinPricesCacheKey, prices, CacheDuration);

            return prices;
        }
    }
}
