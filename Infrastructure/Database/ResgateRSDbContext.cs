using Microsoft.EntityFrameworkCore;
using ResgateRS.Domain.Application.Entities;
using ResgateRS.Infrastructure.Database.Mapping;

namespace ResgateRS.Infrastructure.Database
{
    public class ResgateRSDbContext(DbContextOptions<ResgateRSDbContext> options) : DbContext(options)
    {
        public DbSet<RescueEntity> Rescues => Set<RescueEntity>();
        public DbSet<UserEntity> Users => Set<UserEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RescueMapping());
            modelBuilder.ApplyConfiguration(new UserMapping());
        }
    }
}