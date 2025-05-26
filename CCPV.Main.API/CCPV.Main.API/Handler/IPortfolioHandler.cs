using CCPV.Main.API.Data.Entities;

namespace CCPV.Main.API.Handler
{
    public interface IPortfolioHandler
    {
        public Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, string portfolioName, IFormFile file);
        Task<PortfolioEntity?> GetNoTrackingPortfolioByIdAsync(Guid userId, Guid id);
        Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId);
        public Task<PortfolioEntity> UploadPortfolioFromPathAsync(Guid userId, string portfolioName, string filePath);
        public Task DeletePortfolioAsync(Guid userId, Guid id);
    }
}