using Hangfire;
using PharmacyProject.Application;
using PharmacyProject.Infrastructure;
using PharmacyProject.Infrastructure.Persistence;
using PharmacyProject.Infrastructure.Persistence.Context;
using PharmacyProject.Presentation;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using NpgsqlTypes;

// 1. ÇEVRE DEĞİŞKENLERİ
DotNetEnv.Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

// SERILOG YAPILANDIRMASI
string connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") 
                          ?? builder.Configuration.GetConnectionString("DefaultConnection")!;

IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "MessageTemplate", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
    { "Timestamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    { "Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "Properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "CreatedAt", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) } // NOT NULL olduğu için aynı zamanı basıyoruz
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Gereksiz framework loglarını gizle
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: false
    )
    .CreateLogger();

builder.Host.UseSerilog();

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
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PharmacyProject.Application.Interfaces.Security.IPasswordHasher>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var superAdminDefaultPassword = config["SuperAdminDefaultPassword"] ?? "SuperAdmin123!";
    
    await DatabaseSeeder.SeedCitiesAndDistrictsAsync(context);
    await DatabaseSeeder.SeedPharmaciesAsync(context);
    await DatabaseSeeder.SeedInsuranceCompaniesAsync(context);
    await DatabaseSeeder.SeedSuperAdminAsync(context, passwordHasher, superAdminDefaultPassword);
}

// 5. HANGFIRE TIMERS
PharmacyProject.Infrastructure.BackgroundJobs.HangfireJobScheduler.ScheduleRecurringJobs();

app.Run();