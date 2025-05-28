using CCPV.Main.API.Misc;

namespace CCPV.Main.API.Handler
{
    public interface ICoinHandler
    {
        /// <summary>
        /// Gets the current prices of coins from the Coinlore API.
        /// </summary>
        /// <param name="forceRefresh">Force refreshing the cache</param>
        /// <returns></returns>
        Task<IEnumerable<CoinPrice>> GetPricesAsync(bool forceRefresh, int start, int limit);

        /// <summary>
        /// Gets the prices of specific coins by their Coinlore IDs.
        /// </summary>
        /// <param name="ids">List of Coinlore IDs</param>
        /// <returns></returns>
        Task<IEnumerable<CoinPrice>> GetPricesBySymbolsAsync(IEnumerable<string> symbols);

        /// <summary>
        /// Gets metadata for all coins, including their current prices.
        /// </summary>
        /// <param name="forceRefresh">Force refresh the cache</param>
        /// <returns></returns>
        Task<IEnumerable<CoinPrice>> GetAllCoinsMetadataAsync(bool forceRefresh = false);

    }
}