using Microsoft.EntityFrameworkCore;
using CCPV.Main.API.Data.Entities;

namespace CCPV.Main.API.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }

        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
