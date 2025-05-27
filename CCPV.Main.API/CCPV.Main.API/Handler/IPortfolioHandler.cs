using CCPV.Main.API.Data.Entities;

namespace CCPV.Main.API.Handler
{
    public interface IPortfolioHandler
    {
        Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, string portfolioName, IFormFile file);
        Task<PortfolioEntity?> GetNoTrackingPortfolioByIdAsync(Guid userId, Guid id);
        Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId);
        Task<PortfolioEntity> UploadPortfolioFromPathAsync(Guid userId, string portfolioName, string filePath);
        Task DeletePortfolioAsync(Guid userId, Guid id);
    }
}