using CCPV.Main.API.Clients;
using CCPV.Main.API.Clients.Models;
using CCPV.Main.API.Misc;
using Microsoft.Extensions.Caching.Memory;

namespace CCPV.Main.API.Handler
{
    public class CoinHandler(ICoinloreApi coinloreApi, IMemoryCache cache, ILogger<CoinHandler> logger) : ICoinHandler
    {
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
        private const int MaxBatchSize = 100;

        public async Task<IEnumerable<CoinPrice>> GetPricesAsync(bool forceRefresh, int start, int limit)
        {
            try
            {
                logger.LogInformation($"START: CoinHandler.GetPricesAsync forceRefresh: {forceRefresh}");

                if (limit > 1000)
                {
                    throw new ArgumentException("Limit cannot exceed 1000.");
                }

                string cacheKey = GetCacheKey(start, limit);

                if (!forceRefresh && cache.TryGetValue(cacheKey, out IEnumerable<CoinPrice> cached))
                {
                    return cached;
                }

                List<CoinPrice> allPrices = [];
                int remaining = limit;
                int currentStart = start;

                while (remaining > 0)
                {
                    int batchSize = Math.Min(MaxBatchSize, remaining);
                    logger.LogInformation($"Fetching batch: start={currentStart}, limit={batchSize}");

                    CoinloreResponse response = await coinloreApi.GetTickersAsync(currentStart, batchSize);

                    if (response?.Data == null || !response.Data.Any())
                    {
                        logger.LogWarning("No more data available from Coinlore API.");
                        break;
                    }

                    allPrices.AddRange(response.Data.Select(e => new CoinPrice
                    {
                        Symbol = e.Symbol,
                        Name = e.Name,
                        PriceUsd = e.PriceUsd
                    }));

                    currentStart += batchSize;
                    remaining -= batchSize;
                }

                // Cache the result
                cache.Set(cacheKey, allPrices, CacheDuration);

                return allPrices;
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

        private static string GetCacheKey(int start, int limit)
        {
            return $"{CoinPricesCacheKey}_{start}_{limit}";
        }
    }
}
