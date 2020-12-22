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

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Warning()
#else
                .MinimumLevel.Information()
#endif
                .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
                .Enrich.FromLogContext()
                .WriteTo.SQLite("sbsletl_logs.db", retentionPeriod: TimeSpan.FromDays(31), rollOver:false)
                .WriteTo.Console()
                .WriteTo.RollingFile(formatter, Path.Combine(Directory.GetCurrentDirectory(), "logs/{Date}-SBSLETL.log"),
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
