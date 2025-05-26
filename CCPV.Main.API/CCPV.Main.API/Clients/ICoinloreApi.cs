using CCPV.Main.API.Clients.Models;
using Refit;

namespace CCPV.Main.API.Clients
{
    public interface ICoinloreApi
    {
        [Get("/tickers/")]
        Task<CoinloreResponse> GetTickersAsync();
    }
}
