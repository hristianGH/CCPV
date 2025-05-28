using CCPV.Main.API.Misc;

namespace CCPV.Main.API.Handler
{
    public interface IPortfolioHandler
    {
        Task<ProcessPortfolioResponse> UploadPortfolioAsync(string userName, string portfolioName, IFormFile file);
        Task<ProcessPortfolioResponse> UploadPortfolioFromPathAsync(string userName, string portfolioName, string filePath);
        Task<PortfolioResponse?> GetPortfolioByIdAsync(string userName, Guid portfolioId);
        Task<IEnumerable<PortfolioResponse>> GetPortfoliosByUserIdAsync(string userName);
        Task DeletePortfolioAsync(string userName, Guid portfolioId);
    }
}