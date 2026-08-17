using Hangfire;
using PharmacyProject.Application;
using PharmacyProject.Infrastructure;
using PharmacyProject.Infrastructure.Persistence;
using PharmacyProject.Infrastructure.Persistence.Context;
using PharmacyProject.Presentation;

// 1. ÇEVRE DEĞİŞKENLERİ
DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

// 2. KATMANLARI SİSTEME KAYDET
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPresentationServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<PharmacyProject.Presentation.Middlewares.GlobalExceptionMiddleware>();

// 3. HTTP İSTEK HATTI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

app.UseRateLimiter(); // <--- RATE LIMITER EKLENDİ

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});

app.MapControllers();

// 4. BAŞLANGIÇ VERİLERİ
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.SeedCitiesAndDistrictsAsync(context);
    await DatabaseSeeder.SeedPharmaciesAsync(context);
    await DatabaseSeeder.SeedInsuranceCompaniesAsync(context);
}

// 5. HANGFIRE TIMERS
PharmacyProject.Infrastructure.BackgroundJobs.HangfireJobScheduler.ScheduleRecurringJobs();

app.Run();