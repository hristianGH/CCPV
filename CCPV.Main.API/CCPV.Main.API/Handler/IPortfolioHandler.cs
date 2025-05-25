using CCPV.Main.API.Data.Entities;

namespace CCPV.Main.API.Handler
{
    public interface IPortfolioHandler
    {
        Task<PortfolioEntity> UploadPortfolioAsync(Guid userId, PortfolioEntity portfolio);
        Task<PortfolioEntity?> GetPortfolioByIdAsync(Guid id);
        Task<IEnumerable<PortfolioEntity>> GetPortfoliosByUserIdAsync(Guid userId);
        Task<bool> DeletePortfolioAsync(Guid id);
    }
}