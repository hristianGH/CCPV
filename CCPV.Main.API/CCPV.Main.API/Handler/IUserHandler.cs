namespace CCPV.Main.API.Handler
{
    public interface IUserHandler
    {
        Task<bool> IsUserAdminAsync(Guid userId);
        Task<bool> IsUserPortfolioOwnerAsync(Guid userId, Guid portfolioId);
    }
}
