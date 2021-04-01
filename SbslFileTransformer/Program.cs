using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Display;
using System;
using System.Globalization;
using System.IO;

namespace SbslFileTransformer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var formatter = new MessageTemplateTextFormatter(
                "${Timestamp} [{Level}] {Message:l}{NewLine:l}{Exception:l}", CultureInfo.CurrentCulture);

            var logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SBSL_ETL", "logs");
            var logPathFiles = Path.Combine(logsFolder, "log_files");
            var logPathSqlite = Path.Combine(logsFolder, "log_sqlite");

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Information()
#else
                .MinimumLevel.Information()
#endif
                .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
                .Enrich.FromLogContext()
                .WriteTo.SQLite(Path.Combine(logPathSqlite, "sbsletl_logs.db"), retentionPeriod: TimeSpan.FromDays(10), rollOver:false, maxDatabaseSize: 20480)
                .WriteTo.Console()
                .WriteTo.RollingFile(formatter, Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(logPathFiles, "{Date}-SBSLETL.log")),
                    fileSizeLimitBytes: 10485760)
                .CreateLogger();


            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
