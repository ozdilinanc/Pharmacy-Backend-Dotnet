using PharmacyProject.Application.Interfaces.Security;
using PharmacyProject.Infrastructure.Persistence;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Presentation.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task ApplyDatabaseSeedingAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var superAdminDefaultPassword = config["SuperAdminDefaultPassword"] ?? "SuperAdmin123!";
                
                await DatabaseSeeder.SeedCitiesAndDistrictsAsync(context);
                await DatabaseSeeder.SeedPharmaciesAsync(context);
                await DatabaseSeeder.SeedInsuranceCompaniesAsync(context);
                await DatabaseSeeder.SeedSuperAdminAsync(context, passwordHasher, superAdminDefaultPassword);
            }
        }
    }
}
