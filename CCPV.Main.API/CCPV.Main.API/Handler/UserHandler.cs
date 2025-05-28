
using CCPV.Main.API.Data;
using CCPV.Main.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCPV.Main.API.Handler
{
    public class UserHandler(ApiDbContext dbContext, ILogger<UserHandler> logger) : IUserHandler
    {

        public Task<bool> IsUserAdminAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserPortfolioOwnerAsync(Guid userId, Guid portfolioId)
        {
            throw new NotImplementedException();
        }
        public async Task<Guid> GetOrCreateUserIdAsync(string userName)
        {
            try
            {
                logger.LogInformation("START: UserHandler.GetOrCreateUserIdAsync for userName: {UserName}", userName);


                UserEntity? user = await dbContext.Users
                        .FirstOrDefaultAsync(u => u.Email == userName);

                if (user != null)
                    return user.Id;

                user = new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Email = userName
                };

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("Created new user for username: {UserName}", userName);
                return user.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR: UserHandler.GetOrCreateUserIdAsync for userName: {UserName}", userName);
                throw;
            }
            finally
            {
                logger.LogInformation("END: UserHandler.GetOrCreateUserIdAsync for userName: {UserName}", userName);
            }
        }
    }
}
