using Microsoft.Extensions.DependencyInjection;
using PharmacyProject.Application.Interfaces.Services;
using PharmacyProject.Application.Services;

namespace PharmacyProject.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application'ın Beyni: İş Kuralları (Servisler)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPharmacyService, PharmacyService>();

        return services;
    }
}