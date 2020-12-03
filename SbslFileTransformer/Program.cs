using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
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
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Warning()
#endif
                .Enrich.FromLogContext()
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
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
