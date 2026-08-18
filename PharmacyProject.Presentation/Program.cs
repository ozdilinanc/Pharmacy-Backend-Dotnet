using Hangfire;
using PharmacyProject.Application;
using PharmacyProject.Infrastructure;
using PharmacyProject.Presentation;
using PharmacyProject.Presentation.Extensions;

// 1. ÇEVRE DEĞİŞKENLERİ
DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

// 2. SERILOG YAPILANDIRMASI
builder.AddSerilogConfiguration();

// 3. KATMANLARI SİSTEME KAYDET
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

// 4. HTTP İSTEK HATTI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();

// 5. CORS
app.UseCors("AllowAll");

app.UseRateLimiter(); 

app.UseAuthentication();
app.UseAuthorization();

// 6. Hangfire Dashboard
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});

app.MapControllers();

// 7. BAŞLANGIÇ VERİLERİ
await app.ApplyDatabaseSeedingAsync();

// 8. HANGFIRE TIMERS
PharmacyProject.Infrastructure.BackgroundJobs.HangfireJobScheduler.ScheduleRecurringJobs();

app.Run();