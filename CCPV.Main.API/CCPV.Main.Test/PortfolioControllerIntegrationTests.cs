using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Handler;
using CCPV.Main.API.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CCPV.Main.Test
{
    public class PortfolioControllerIntegrationTests
    {
        private readonly ITestOutputHelper _output;
        private readonly string _dbName;
        private readonly ApiDbContext _dbContext;
        private readonly Mock<IUserHandler> _mockUserHandler;
        private readonly ILogger<PortfolioHandler> _logger;
        private readonly PortfolioHandler _handler;
        private readonly TestServer<Startup> _testServer;
        private static string _userName = "TestUser";
        private static Guid _userId = Guid.NewGuid();
        private Dictionary<string, string> _headers = new()
            { { "UserName", _userName } };

        public PortfolioControllerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _dbName = Guid.NewGuid().ToString();
            _dbContext = TestDbContextFactory.CreateInMemoryDbContext(_dbName);
            _mockUserHandler = new Mock<IUserHandler>();
            _logger = new LoggerFactory().CreateLogger<PortfolioHandler>();
            _handler = new PortfolioHandler(_dbContext, _mockUserHandler.Object, _logger);
            _testServer = new TestServer<Startup>((typeof(IPortfolioHandler), _handler));
        }

        [Fact]
        public async Task ProcessFromFile_ReturnsPortfolio()
        {
            ProcessPortfolioFileRequest request = new()
            {
                UserId = Guid.NewGuid(),
                PortfolioName = "TestPortfolio",
                FilePath = "fakepath.csv"
            };
            await File.WriteAllTextAsync("fakepath.csv", "1200|USDT|1123.23\n10000000|SHIB|60");
            _mockUserHandler.Setup(x => x.GetOrCreateUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(request.UserId);
            HttpResponseMessage response = await _testServer.PostAsync("/api/portfolio/process-from-file", request, _headers);
            response.EnsureSuccessStatusCode();

            ProcessPortfolioResponse? result = await response.Content.ReadFromJsonAsync<ProcessPortfolioResponse>();

            Assert.NotNull(response);
            Assert.Equal("Portfolio processing started", result.Message);
            File.Delete("fakepath.csv");
        }

        [Fact]
        public async Task GetPortfoliosByUserId_ReturnsList()
        {
            PortfolioEntity portfolio = new() { Id = Guid.NewGuid(), Name = "P1", UserId = _userId, CreatedAt = DateTime.UtcNow, Entries = [] };
            _dbContext.Portfolios.Add(portfolio);
            _dbContext.SaveChanges();
            _mockUserHandler.Setup(x => x.GetOrCreateUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_userId);
            HttpResponseMessage response = await _testServer.GetAsync($"/api/portfolio", _headers);

            response.EnsureSuccessStatusCode();
            List<PortfolioResponse>? result = await response.Content.ReadFromJsonAsync<List<PortfolioResponse>>();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("P1", result[0].Name);
        }

        [Fact]
        public async Task ProcessFromFile_ReturnsPortfolio_WithRealHandler()
        {
            ProcessPortfolioFileRequest request = new()
            {
                UserId = Guid.NewGuid(),
                PortfolioName = "TestPortfolio",
                FilePath = "fakepath.csv"
            };

            await File.WriteAllTextAsync("fakepath.csv", "10|ETH|123.14\n12.12454|BTC|24012.43");
            _mockUserHandler.Setup(x => x.GetOrCreateUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(request.UserId);
            HttpResponseMessage response = await _testServer.PostAsync("/api/portfolio/process-from-file", request, _headers);
            response.EnsureSuccessStatusCode();
            ProcessPortfolioResponse? result = await response.Content.ReadFromJsonAsync<ProcessPortfolioResponse>();

            Assert.NotNull(result);
            Assert.Equal("Portfolio processing started", result.Message);

            // Cleanup
            File.Delete("fakepath.csv");
        }
    }
}
