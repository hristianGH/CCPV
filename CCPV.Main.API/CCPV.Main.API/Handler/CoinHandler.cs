using CCPV.Main.API.Clients;
using CCPV.Main.API.Clients.Models;
using CCPV.Main.API.Misc;
using Microsoft.Extensions.Caching.Memory;

namespace CCPV.Main.API.Handler
{
    public class CoinHandler(ICoinloreApi coinloreApi, IMemoryCache cache, ILogger<CoinHandler> logger) : ICoinHandler
    {
        private const string CoinPricesCacheKey = "CoinPrices";
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

        public async Task<IEnumerable<CoinPrice>> GetPricesAsync(bool forceRefresh = false)
        {
            try
            {
                logger.LogInformation($"START: CoinHandler.GetPricesAsync forceRefresh: {forceRefresh}");
                if (!forceRefresh && cache.TryGetValue(CoinPricesCacheKey, out IEnumerable<CoinPrice> cached))
                {
                    return cached;
                }
                // Fetch from Coinlore API
                CoinloreResponse response = await coinloreApi.GetTickersAsync();

                List<CoinPrice> prices = [.. response.Data
                            .Select(e => new CoinPrice
                            {
                                Symbol = e.Symbol,
                                Name = e.Name,
                                PriceUsd = e.PriceUsd
                            })];

                cache.Set(CoinPricesCacheKey, prices, CacheDuration);

                return prices;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: CoinHandler.GetPricesAsync forceRefresh: {forceRefresh}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: CoinHandler.GetPricesAsync forceRefresh: {forceRefresh}");
            }
        }
    }
}
