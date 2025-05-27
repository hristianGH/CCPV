using CCPV.Main.API.Clients;
using CCPV.Main.API.Clients.Models;
using CCPV.Main.API.Misc;
using Moq;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CCPV.Main.Test
{
    public class CoinControllerIntegrationTests
    {
        private readonly ITestOutputHelper _output;
        public CoinControllerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task GetPricesByIds_ReturnsExpectedCoins()
        {
            // Arrange
            Mock<ICoinloreApi> mockApi = new();
            mockApi.Setup(api => api.GetTickersByIdsAsync(It.IsAny<string>()))
                .ReturnsAsync((string ids) =>
                {
                    string[] idList = ids.Split(',');
                    List<CoinloreCoin> coins = [];
                    foreach (string id in idList)
                    {
                        coins.Add(new CoinloreCoin
                        {
                            Id = id,
                            Symbol = $"SYM{id}",
                            Name = $"Coin{id}",
                            PriceUsdRaw = "123.45"
                        });
                    }
                    return coins;
                });

            TestServer<Startup> testServer = new((typeof(ICoinloreApi), mockApi.Object));

            // Act
            HttpResponseMessage response = await testServer.GetAsync("/api/coin/by-ids?ids=90,80");
            response.EnsureSuccessStatusCode();
            List<CoinPrice>? coins = await response.Content.ReadFromJsonAsync<List<CoinPrice>>();

            // Assert
            Assert.NotNull(coins);
            Assert.Equal(2, coins.Count);
            Assert.Contains(coins, c => c.Symbol == "SYM90");
            Assert.Contains(coins, c => c.Symbol == "SYM80");
        }

        [Fact]
        public async Task GetPrices_ReturnsBatchOfCoins()
        {
            // Arrange
            Mock<ICoinloreApi> mockApi = new();
            mockApi.Setup(api => api.GetTickersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((int start, int limit) =>
                {
                    CoinloreResponse response = new()
                    {
                        Data = []
                    };
                    for (int i = start; i < start + limit; i++)
                    {
                        response.Data.Add(new CoinloreCoin
                        {
                            Id = i.ToString(),
                            Symbol = $"SYM{i}",
                            Name = $"Coin{i}",
                            PriceUsdRaw = "100.00"
                        });
                    }
                    return response;
                });

            TestServer<Startup> testServer = new((typeof(ICoinloreApi), mockApi.Object));

            // Act
            HttpResponseMessage response = await testServer.GetAsync("/api/coin/prices?start=0&limit=3");
            response.EnsureSuccessStatusCode();
            List<CoinPrice>? coins = await response.Content.ReadFromJsonAsync<List<CoinPrice>>();

            // Assert
            Assert.NotNull(coins);
            Assert.Equal(3, coins.Count);
            Assert.Contains(coins, c => c.Symbol == "SYM0");
            Assert.Contains(coins, c => c.Symbol == "SYM1");
            Assert.Contains(coins, c => c.Symbol == "SYM2");
        }
    }
}
