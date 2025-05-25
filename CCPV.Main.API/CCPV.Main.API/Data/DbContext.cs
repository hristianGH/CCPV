using CCPV.Main.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCPV.Main.API.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }

        public DbSet<PortfolioEntity> Portfolios { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<PortfolioEntryEntity> PortfolioEntries { get; set; }
        public DbSet<UploadStatusEntity> UploadStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API for relationships (optional if using conventions)

            modelBuilder.Entity<UserEntity>()
                .HasMany(u => u.Portfolios)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PortfolioEntity>()
                .HasMany(p => p.Entries)
                .WithOne(e => e.Portfolio)
                .HasForeignKey(e => e.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
