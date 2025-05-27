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

        public PortfolioControllerIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _dbName = Guid.NewGuid().ToString();
            _dbContext = TestDbContextFactory.CreateInMemoryDbContext(_dbName);
            _mockUserHandler = new Mock<IUserHandler>();
            _logger = new LoggerFactory().CreateLogger<PortfolioHandler>();
            _handler = new PortfolioHandler(_dbContext, _mockUserHandler.Object, _logger);
            _testServer = new TestServer<Startup>(_output, (typeof(IPortfolioHandler), _handler));
        }

        [Fact]
        public async Task ProcessFromFile_ReturnsPortfolio()
        {
            // Arrange
            ProcessPortfolioFileRequest request = new()
            {
                UserId = Guid.NewGuid(),
                PortfolioName = "TestPortfolio",
                FilePath = "fakepath.csv"
            };
            await File.WriteAllTextAsync("fakepath.csv", "BTC,0.5,10000\nETH,2,2000");

            // Act
            HttpResponseMessage response = await _testServer.PostAsync("/api/1portfolio/process-from-file", request);
            response.EnsureSuccessStatusCode();
            PortfolioEntity? portfolio = await response.Content.ReadFromJsonAsync<PortfolioEntity>();

            // Assert
            Assert.NotNull(portfolio);
            Assert.Equal("TestPortfolio", portfolio.Name);
            Assert.Equal(2, portfolio.Entries.Count);

            // Cleanup
            File.Delete("fakepath.csv");
        }

        [Fact]
        public async Task GetPortfoliosByUserId_ReturnsList()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            PortfolioEntity portfolio = new() { Id = Guid.NewGuid(), Name = "P1", UserId = userId, CreatedAt = DateTime.UtcNow, Entries = [] };
            _dbContext.Portfolios.Add(portfolio);
            _dbContext.SaveChanges();

            // Act
            HttpResponseMessage response = await _testServer.GetAsync($"/api/1portfolio?userId={userId}");
            response.EnsureSuccessStatusCode();
            List<PortfolioEntity>? result = await response.Content.ReadFromJsonAsync<List<PortfolioEntity>>();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("P1", result[0].Name);
        }

        [Fact]
        public async Task ProcessFromFile_ReturnsPortfolio_WithRealHandler()
        {
            // Arrange
            ProcessPortfolioFileRequest request = new()
            {
                UserId = Guid.NewGuid(),
                PortfolioName = "TestPortfolio",
                FilePath = "fakepath.csv"
            };

            // Create a fake file for the handler to read
            await File.WriteAllTextAsync("fakepath.csv", "BTC,0.5,10000\nETH,2,2000");

            // Act
            HttpResponseMessage response = await _testServer.PostAsync("/api/1portfolio/process-from-file", request);
            response.EnsureSuccessStatusCode();
            PortfolioEntity? portfolio = await response.Content.ReadFromJsonAsync<PortfolioEntity>();

            // Assert
            Assert.NotNull(portfolio);
            Assert.Equal("TestPortfolio", portfolio.Name);
            Assert.Equal(2, portfolio.Entries.Count);

            // Cleanup
            File.Delete("fakepath.csv");
        }
    }
}
