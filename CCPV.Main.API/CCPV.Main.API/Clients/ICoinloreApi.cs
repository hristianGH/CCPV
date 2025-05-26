namespace CCPV.Main.API.Clients
{
    public interface ICoinloreApi
    {
        [Get("/tickers/")]
        Task<CoinloreResponse> GetTickersAsync();
    }
}
