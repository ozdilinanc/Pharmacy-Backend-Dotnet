using Microsoft.EntityFrameworkCore;
using PharmacyProject.Core.Entities;

namespace PharmacyProject.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        //options is a parameter that contains configuration information for the DbContext in appsettings json file.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<UnmatchedPharmacy> UnmatchedPharmacies { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
