using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmacyProject.Application.Interfaces.External;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Security;
using PharmacyProject.Application.Interfaces.Services;
using PharmacyProject.Application.Services;
using PharmacyProject.Infrastructure.ExternalServices;
using PharmacyProject.Infrastructure.Persistence;
using PharmacyProject.Infrastructure.Persistence.Context;
using PharmacyProject.Infrastructure.Persistence.Repositories;
using PharmacyProject.Infrastructure.Security;
using System.Text;

namespace PharmacyProject.Presentation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                                   ?? builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                            ?? builder.Configuration["Jwt:Secret"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
                };
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pharmacy API", Version = "v1" });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Örnek kullanım: 'Bearer eyJhbGciOi...'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICityRepository, CityRepository>();
            builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
            builder.Services.AddScoped<IPharmacyRepository, PharmacyRepository>();
            builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
            builder.Services.AddScoped<IPharmacyInsuranceRepository, PharmacyInsuranceRepository>();
            builder.Services.AddScoped<IUnmatchedPharmacyRepository, UnmatchedPharmacyRepository>();

            builder.Services.AddScoped<IPharmacyService, PharmacyService>();
            builder.Services.AddHttpClient<INosyApiService, NosyApiService>();

            // builder.Services.AddHostedService<PharmacyProject.Infrastructure.Workers.RecentPharmacySyncWorker>();
            // builder.Services.AddHostedService<PharmacyProject.Infrastructure.Workers.OnDutyPharmacySyncWorker>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers(); // Fazladan yazılan ikinci app.MapControllers() satırı silindi.

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await DatabaseSeeder.SeedCitiesAndDistrictsAsync(context);
                await DatabaseSeeder.SeedPharmaciesAsync(context);
            }

            app.Run();
        }
    }
}