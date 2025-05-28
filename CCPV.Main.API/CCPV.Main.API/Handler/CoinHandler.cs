using CCPV.Main.API.Clients;
using CCPV.Main.API.Clients.Models;
using CCPV.Main.API.Misc;
using Microsoft.Extensions.Caching.Memory;

namespace CCPV.Main.API.Handler
{
    public class CoinHandler(ICoinloreApi coinloreApi, IMemoryCache cache, ILogger<CoinHandler> logger) : ICoinHandler
    {
        private TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
        private const int MaxBatchSize = 100;
        private const string CoinPricesCacheKey = "CoinPrices";
        private const string CoinPriceBySymbolCacheKey = "CoinPriceBySymbol";
        private const string AllCoinsCacheKey = "AllCoinMetadata";

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
                        Id = e.Id,
                        Symbol = e.Symbol,
                        Name = e.Name,
                        PriceUsd = e.PriceUsd
                    }));
                    foreach (CoinPrice coin in allPrices)
                    {
                        string coinKey = GetCoinCacheKey(coin.Symbol);
                        cache.Set(coinKey, coin, CacheDuration);
                    }
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
        public async Task<IEnumerable<CoinPrice>> GetPricesBySymbolsAsync(IEnumerable<string> symbols)
        {
            IEnumerable<CoinPrice> allCoins = await GetAllCoinsMetadataAsync();
            Dictionary<string, string> symbolToId = allCoins
                .Where(c => symbols.Contains(c.Symbol, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(c => c.Symbol, c => c.Id, StringComparer.OrdinalIgnoreCase);

            List<string> ids = symbols
                .Where(s => symbolToId.ContainsKey(s))
                .Select(s => symbolToId[s])
                .ToList();

            return await GetPricesByIdsAsync(ids);
        }
        private async Task<IEnumerable<CoinPrice>> GetPricesByIdsAsync(IEnumerable<string> ids)
        {
            List<string> idList = ids.Distinct().ToList();
            List<CoinPrice> result = [];
            List<string> idsToFetch = [];

            foreach (string? id in idList)
            {
                string coinKey = $"CoinPriceById_{id}";
                if (cache.TryGetValue(coinKey, out CoinPrice? cachedCoin) && cachedCoin != null)
                {
                    result.Add(cachedCoin);
                }
                else
                {
                    idsToFetch.Add(id);
                }
            }

            if (idsToFetch.Count > 0)
            {
                string idsParam = string.Join(",", idsToFetch);
                List<CoinloreCoin> apiResult = await coinloreApi.GetTickersByIdsAsync(idsParam);
                if (apiResult != null)
                {
                    foreach (CoinloreCoin coin in apiResult)
                    {
                        CoinPrice coinPrice = new()
                        {
                            Id = coin.Id,
                            Symbol = coin.Symbol,
                            Name = coin.Name,
                            PriceUsd = coin.PriceUsd
                        };
                        string coinKey = $"CoinPriceById_{coin.Id}";
                        cache.Set(coinKey, coinPrice, CacheDuration);
                        result.Add(coinPrice);
                    }
                }
            }
            return result;
        }

        public async Task<IEnumerable<CoinPrice>> GetAllCoinsMetadataAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && cache.TryGetValue(AllCoinsCacheKey, out IEnumerable<CoinPrice> cachedCoins))
                return cachedCoins;

            List<CoinPrice> allCoins = [];
            int start = 0;
            const int batchSize = 100;

            while (true)
            {
                CoinloreResponse response = await coinloreApi.GetTickersAsync(start, batchSize);
                if (response?.Data == null || !response.Data.Any())
                    break;

                allCoins.AddRange(response.Data.Select(c => new CoinPrice
                {
                    Id = c.Id,
                    Symbol = c.Symbol,
                    Name = c.Name
                }));

                if (response.Data.Count < batchSize)
                    break;

                start += batchSize;
            }

            cache.Set(AllCoinsCacheKey, allCoins, TimeSpan.FromDays(1));
            return allCoins;
        }

        private static string GetCacheKey(int start, int limit)
        {
            return $"{CoinPricesCacheKey}_{start}_{limit}";
        }
        private static string GetCoinCacheKey(string symbol)
        {
            return $"{CoinPriceBySymbolCacheKey}_{symbol}";
        }
    }
}
