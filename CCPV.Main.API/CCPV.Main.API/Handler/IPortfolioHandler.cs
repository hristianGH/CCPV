using CCPV.Main.API.Data.Entities;

namespace CCPV.Main.API.Handler
{
    public interface IPortfolioHandler
    {
        public Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, string portfolioName, IFormFile file);
        Task ProcessPortfolioAsync(Guid userId, PortfolioEntity portfolio);
        Task<PortfolioEntity?> GetNoTrackingPortfolioByIdAsync(Guid id);
        Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId);
        Task<bool> DeletePortfolioAsync(Guid id);
    }
}