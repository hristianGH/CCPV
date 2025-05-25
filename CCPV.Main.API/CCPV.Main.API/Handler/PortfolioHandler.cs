using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCPV.Main.API.Handler
{
    public class PortfolioHandler : IPortfolioHandler
    {
        private readonly ApiDbContext _dbContext;

        public PortfolioHandler(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, string portfolioName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            List<PortfolioEntryEntity> entries = await ParsePortfolioFileAsync(file);

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

            _dbContext.Portfolios.Add(portfolio);
            await _dbContext.SaveChangesAsync();

            return portfolio;
        }

        private async Task<List<PortfolioEntryEntity>> ParsePortfolioFileAsync(IFormFile file)
        {
            List<PortfolioEntryEntity> entries = [];

            using Stream stream = file.OpenReadStream();
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

        public async Task<PortfolioEntity?> GetPortfolioByIdAsync(Guid id)
        {
            return await _dbContext.Portfolios
                .Include(p => p.Entries)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId)
        {
            return await _dbContext.Portfolios
                .Include(p => p.Entries)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> DeletePortfolioAsync(Guid id)
        {
            PortfolioEntity? portfolio = await _dbContext.Portfolios.FindAsync(id);
            if (portfolio == null) return false;

            _dbContext.Portfolios.Remove(portfolio);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
