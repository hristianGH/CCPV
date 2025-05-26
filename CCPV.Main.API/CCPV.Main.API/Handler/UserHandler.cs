
namespace CCPV.Main.API.Handler
{
    public class UserHandler : IUserHandler
    {
        public Task<bool> IsUserAdminAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserPortfolioOwnerAsync(Guid userId, Guid portfolioId)
        {
            throw new NotImplementedException();
        }
    }
}
