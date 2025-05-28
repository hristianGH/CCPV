using CCPV.Main.API.Clients.Models;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace CCPV.Main.API.Clients
{
    public interface ICoinloreApi
    {
        [Get("/tickers/")]
        Task<CoinloreResponse> GetTickersAsync(
            int start = 0,
            int limit = 100);

        [Get("/ticker/")]
        Task<List<CoinloreCoin>> GetTickersByIdsAsync([FromQuery] string id);
    }
}
