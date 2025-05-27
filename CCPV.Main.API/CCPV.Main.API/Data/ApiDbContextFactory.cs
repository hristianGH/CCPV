using CCPV.Main.API.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CCPV.Main.API.Data
{
    public class ApiDbContextFactory : IDesignTimeDbContextFactory<ApiDbContext>
    {
        public ApiDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            DbContextOptionsBuilder<ApiDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer((Environment.GetEnvironmentVariable(Constants.RemoteSqlConnection) ??
            config.GetConnectionString(Constants.DefaultConnection)));

            return new ApiDbContext(optionsBuilder.Options);
        }
    }
}