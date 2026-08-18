using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

namespace PharmacyProject.Presentation.Extensions
{
    public static class SerilogExtensions
    {
        public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
        {
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
                { "CreatedAt", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) } 
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) 
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
        }
    }
}
