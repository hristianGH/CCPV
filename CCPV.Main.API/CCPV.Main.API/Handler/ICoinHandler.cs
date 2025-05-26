using CCPV.Main.API.Misc;

namespace CCPV.Main.API.Handler
{
    public interface ICoinHandler
    {
        Task<IEnumerable<CoinPrice>> GetPricesAsync(bool forceRefresh = false);
    }
}