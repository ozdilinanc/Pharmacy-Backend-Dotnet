using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Security;
using PharmacyProject.Infrastructure.ExternalServices;
using PharmacyProject.Infrastructure.Persistence.Context;
using PharmacyProject.Infrastructure.Persistence.Repositories;
using PharmacyProject.Infrastructure.Security;

namespace PharmacyProject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                               ?? configuration.GetConnectionString("DefaultConnection");

        // 1. Veritabanı
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. Repositories (Kaslar)
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IDistrictRepository, DistrictRepository>();
        services.AddScoped<IPharmacyRepository, PharmacyRepository>();
        services.AddScoped<IInsuranceRepository, InsuranceRepository>();
        services.AddScoped<IPharmacyInsuranceRepository, PharmacyInsuranceRepository>();
        services.AddScoped<IUnmatchedPharmacyRepository, UnmatchedPharmacyRepository>();

        // 3. Dış API'ler
        services.AddHttpClient<INosyApiService, NosyApiService>();

        // 4. Güvenlik Altyapısı
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        // 5. Hangfire (Arka Plan İşçisi)
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();

        return services;
    }
}