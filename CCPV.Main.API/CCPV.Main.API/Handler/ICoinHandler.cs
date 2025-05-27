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
        Task<IEnumerable<CoinPrice>> GetPricesAsync(bool forceRefresh = false);
    }
}