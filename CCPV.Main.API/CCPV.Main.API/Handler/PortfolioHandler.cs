using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCPV.Main.API.Handler
{
    public class PortfolioHandler(ApiDbContext dbContext, IUserHandler userHandler) : IPortfolioHandler
    {
        public async Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, string portfolioName, IFormFile file)
        {
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

        public async Task<PortfolioEntity> UploadPortfolioFromPathAsync(Guid userId, string portfolioName, string filePath)
        {
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

        private async Task<List<PortfolioEntryEntity>> ParsePortfolioFileAsync(Stream stream)
        {
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

        public async Task<PortfolioEntity?> GetNoTrackingPortfolioByIdAsync(Guid userId, Guid id)
        {
            return await dbContext.Portfolios
                .AsNoTracking()
                .Include(p => p.Entries)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId)
        {
            return await dbContext.Portfolios
                .AsNoTracking()
                .Include(p => p.Entries)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task DeletePortfolioAsync(Guid userId, Guid id)
        {
            PortfolioEntity? portfolio = await dbContext.Portfolios.FindAsync(id);
            if (portfolio == null)
            {
                throw new KeyNotFoundException($"Portfolio with ID {id} not found.");
            }

            dbContext.Portfolios.Remove(portfolio);
            await dbContext.SaveChangesAsync();
        }
    }
}
