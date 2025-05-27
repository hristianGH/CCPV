using CCPV.Main.API.Clients.Models;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace CCPV.Main.API.Clients
{
    public interface ICoinloreApi
    {
        [Get("/tickers/")]
        Task<CoinloreResponse> GetTickersAsync([FromQuery] int currentStart, [FromQuery] int batchSize);

        [Get("/ticker/")]
        Task<List<CoinloreCoin>> GetTickersByIdsAsync([FromQuery] string id);
    }
}
