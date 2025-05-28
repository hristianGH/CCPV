using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using CCPV.Main.API.Misc;
using Microsoft.EntityFrameworkCore;

namespace CCPV.Main.API.Handler
{
    public class PortfolioHandler(ApiDbContext dbContext, IUserHandler userHandler, ILogger<PortfolioHandler> logger) : IPortfolioHandler
    {
        public async Task<ProcessPortfolioResponse> UploadPortfolioAsync(string userName, string portfolioName, IFormFile file)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.UploadPortfolioAsync userId: {userName} portfolioName: {portfolioName}");
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty.");

                List<PortfolioEntryEntity> entries = await ParsePortfolioFileAsync(file.OpenReadStream());
                Guid userId = await userHandler.GetOrCreateUserIdAsync(userName);

                PortfolioEntity portfolio = new()
                {
                    Id = Guid.NewGuid(),
                    Name = portfolioName,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    Entries = entries
                };

                foreach (PortfolioEntryEntity entry in entries)
                {
                    entry.Id = Guid.NewGuid();
                    entry.PortfolioId = portfolio.Id;
                }

                dbContext.Portfolios.Add(portfolio);
                await dbContext.SaveChangesAsync();
                // return ProcessPortfolioResponse with the portfolio ID and a message
                return new ProcessPortfolioResponse
                {
                    Message = "Portfolio uploaded successfully.",
                    PortfolioId = portfolio.Id
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.UploadPortfolioAsync userId: {userName} portfolioName: {portfolioName}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.UploadPortfolioAsync userId: {userName} portfolioName: {portfolioName}");
            }
        }

        public async Task<ProcessPortfolioResponse> UploadPortfolioFromPathAsync(string userName, string portfolioName, string filePath)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.UploadPortfolioFromPathAsync userId:{userName} porfolioName:{portfolioName} filePath: {filePath}");
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Portfolio file not found.", filePath);

                using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
                List<PortfolioEntryEntity> entries = await ParsePortfolioFileAsync(stream);
                Guid userId = await userHandler.GetOrCreateUserIdAsync(userName);
                PortfolioEntity portfolio = new()
                {
                    Id = Guid.NewGuid(),
                    Name = portfolioName,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    Entries = entries
                };

                foreach (PortfolioEntryEntity entry in entries)
                {
                    entry.Id = Guid.NewGuid();
                    entry.PortfolioId = portfolio.Id;
                }

                dbContext.Portfolios.Add(portfolio);
                await dbContext.SaveChangesAsync();
                // To do reurn a background job to process the portfolio
                return new ProcessPortfolioResponse
                {
                    Message = "Portfolio processing started",
                    PortfolioId = portfolio.Id
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.UploadPortfolioFromPathAsync userId:{userName} porfolioName:{portfolioName} filePath: {filePath}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.UploadPortfolioFromPathAsync userId:{userName} porfolioName:{portfolioName} filePath: {filePath}");
            }
        }

        private async Task<List<PortfolioEntryEntity>> ParsePortfolioFileAsync(Stream stream)
        {
            try
            {
                logger.LogInformation("START: PortfolioHandler.ParsePortfolioFileAsync");
                List<PortfolioEntryEntity> entries = [];
                using StreamReader reader = new(stream);

                string? line;
                int lineNumber = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;

                    string[] parts = line.Split('|');
                    if (parts.Length != 3)
                        throw new FormatException($"Invalid format at line {lineNumber}.");

                    if (!decimal.TryParse(parts[0], out decimal amount))
                        throw new FormatException($"Invalid amount at line {lineNumber}.");

                    string coinSymbol = parts[1];

                    if (!decimal.TryParse(parts[2], out decimal buyPrice))
                        throw new FormatException($"Invalid buy price at line {lineNumber}.");

                    entries.Add(new PortfolioEntryEntity
                    {
                        CoinSymbol = coinSymbol,
                        Amount = amount,
                        BuyPrice = buyPrice
                    });
                }

                return entries;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR: PortfolioHandler.ParsePortfolioFileAsync");
                throw;
            }
            finally
            {
                logger.LogInformation("END: PortfolioHandler.ParsePortfolioFileAsync");
            }
        }

        public async Task<PortfolioResponse?> GetPortfolioByIdAsync(string userName, Guid portfolioId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.GetNoTrackingPortfolioByIdAsync userId: {userName} portfolioId: {portfolioId}");
                PortfolioEntity? response = await dbContext.Portfolios
                        .AsNoTracking()
                        .Include(p => p.Entries)
                        .FirstOrDefaultAsync(p => p.Id == portfolioId);

                return response != null ? MapToPortfolioResponse(response) : null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.GetNoTrackingPortfolioByIdAsync userId: {userName} portfolioId: {portfolioId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.GetNoTrackingPortfolioByIdAsync userId: {userName} portfolioId: {portfolioId}");
            }
        }

        public async Task<IEnumerable<PortfolioResponse>> GetPortfoliosByUserIdAsync(string userName)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.GetPortfoliosByUserIdAsync userId:{userName}");
                Guid userId = await userHandler.GetOrCreateUserIdAsync(userName);

                List<PortfolioEntity> response = await dbContext.Portfolios
                    .AsNoTracking()
                    .Include(p => p.Entries)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                return response.Select(MapToPortfolioResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.GetPortfoliosByUserIdAsync userId:{userName}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.GetPortfoliosByUserIdAsync userId:{userName}");
            }
        }

        public async Task DeletePortfolioAsync(string userName, Guid portfolioId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.DeletePortfolioAsync userId: {userName} portfolioId: {portfolioId}");
                Guid userId = await userHandler.GetOrCreateUserIdAsync(userName);
                PortfolioEntity? portfolio = dbContext.Portfolios
                    .Include(p => p.Entries)
                    .FirstOrDefault(p => p.UserId == userId && p.Id == portfolioId);
                if (portfolio == null)
                {
                    throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");
                }

                dbContext.Portfolios.Remove(portfolio);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.DeletePortfolioAsync userId: {userName} portfolioId: {portfolioId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.DeletePortfolioAsync userId: {userName} portfolioId: {portfolioId}");
            }
        }
        private static PortfolioResponse MapToPortfolioResponse(PortfolioEntity entity)
        {
            return new PortfolioResponse
            {
                Name = entity.Name,
                Coins = entity.Entries.Select(MapToEntryResponse).ToList()
            };
        }

        private static PortfolioEntryResponse MapToEntryResponse(PortfolioEntryEntity entry)
        {
            return new PortfolioEntryResponse
            {
                CoinSymbol = entry.CoinSymbol,
                Amount = entry.Amount,
                BuyPrice = entry.BuyPrice
            };
        }
    }
}
