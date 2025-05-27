using CCPV.Main.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CCPV.Main.Test
{
    public static class TestDbContextFactory
    {
        public static ApiDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApiDbContext(options);
        }
    }
}
