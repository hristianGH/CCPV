using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCPV.Main.API.Handler
{
    public class PortfolioHandler(ApiDbContext dbContext, IUserHandler userHandler, ILogger<PortfolioHandler> logger) : IPortfolioHandler
    {
        public async Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, string portfolioName, IFormFile file)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.UploadPortfolioAsync userId: {userId} portfolioName: {portfolioName}");
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty.");

                List<PortfolioEntryEntity> entries = await ParsePortfolioFileAsync(file.OpenReadStream());

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

                return portfolio;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.UploadPortfolioAsync userId: {userId} portfolioName: {portfolioName}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.UploadPortfolioAsync userId: {userId} portfolioName: {portfolioName}");
            }
        }

        public async Task<PortfolioEntity> UploadPortfolioFromPathAsync(Guid userId, string portfolioName, string filePath)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.UploadPortfolioFromPathAsync userId:{userId} porfolioName:{portfolioName} filePath: {filePath}");
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Portfolio file not found.", filePath);

                using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);
                List<PortfolioEntryEntity> entries = await ParsePortfolioFileAsync(stream);

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

                return portfolio;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.UploadPortfolioFromPathAsync userId:{userId} porfolioName:{portfolioName} filePath: {filePath}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.UploadPortfolioFromPathAsync userId:{userId} porfolioName:{portfolioName} filePath: {filePath}");
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

        public async Task<PortfolioEntity?> GetNoTrackingPortfolioByIdAsync(Guid userId, Guid portfolioId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.GetNoTrackingPortfolioByIdAsync userId: {userId} portfolioId: {portfolioId}");
                return await dbContext.Portfolios
                        .AsNoTracking()
                        .Include(p => p.Entries)
                        .FirstOrDefaultAsync(p => p.Id == portfolioId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.GetNoTrackingPortfolioByIdAsync userId: {userId} portfolioId: {portfolioId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.GetNoTrackingPortfolioByIdAsync userId: {userId} portfolioId: {portfolioId}");
            }
        }

        public async Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.GetPortfoliosByUserIdAsync userId:{userId}");
                return await dbContext.Portfolios
                    .AsNoTracking()
                    .Include(p => p.Entries)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.GetPortfoliosByUserIdAsync userId:{userId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.GetPortfoliosByUserIdAsync userId:{userId}");
            }
        }

        public async Task DeletePortfolioAsync(Guid userId, Guid portfolioId)
        {
            try
            {
                logger.LogInformation($"START: PortfolioHandler.DeletePortfolioAsync userId: {userId} portfolioId: {portfolioId}");
                PortfolioEntity? portfolio = await dbContext.Portfolios.FindAsync(portfolioId);
                if (portfolio == null)
                {
                    throw new KeyNotFoundException($"Portfolio with ID {portfolioId} not found.");
                }

                dbContext.Portfolios.Remove(portfolio);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ERROR: PortfolioHandler.DeletePortfolioAsync userId: {userId} portfolioId: {portfolioId}");
                throw;
            }
            finally
            {
                logger.LogInformation($"END: PortfolioHandler.DeletePortfolioAsync userId: {userId} portfolioId: {portfolioId}");
            }
        }
    }
}
