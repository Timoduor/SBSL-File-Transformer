using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SbslFileTransformer.Infrastructure.ServiceManager;
using SbslFileTransformer.Infrastructure.SignalRLogging;

using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Display;
using Serilog.Sinks.MariaDB.Extensions;

namespace SbslFileTransformer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                AddLogging(host);

                Log.Information("Starting up");

                host.Run();
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

        private static void ChangeServiceStartParams()
        {
            try
            {
                LocalServiceHelper.ChangeRevoveryOption("SBSL ETL Service",
                    ServiceRecoveryOptionHelper.RecoverAction.Restart,
                    ServiceRecoveryOptionHelper.RecoverAction.Restart,
                    ServiceRecoveryOptionHelper.RecoverAction.None);
                //#if !DEBUG

                RunServerManager();
                //#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Problem setting service to restart automatically!");
            }
        }

        private static void RunServerManager()
        {
            //var process = Path.Combine(Directory.GetCurrentDirectory(), "SbslServiceManager.exe");

            //ProcessExtensions.StartProcessAsCurrentUser(process);


            //string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            //using (StreamWriter writer = new StreamWriter(deskDir + "\\" + linkName + ".url"))
            //{
            //    string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //    writer.WriteLine("[InternetShortcut]");
            //    writer.WriteLine("URL=file:///" + app);
            //    writer.WriteLine("IconIndex=0");
            //    string icon = app.Replace('\\', '/');
            //    writer.WriteLine("IconFile=" + icon);
            //}


            //File.Copy("shortcut path...", Environment.GetFolderPath(Environment.SpecialFolder.Startup) + shorcutname);
        }

        private static void AddLogging(IHost host)
        {
            var logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SBSL_ETL", "logs");
            var logPathFiles = Path.Combine(logsFolder, "log_files");

            _ = Directory.CreateDirectory(logPathFiles);

            try
            {
                var formatter = new MessageTemplateTextFormatter(
                    "${Timestamp} [{Level}] {Message:l}{NewLine:l}{Exception:l}", CultureInfo.CurrentCulture);

                var connString = host.Services.GetService<IConfiguration>().GetConnectionString("DefaultConnection");

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
                    .Enrich.FromLogContext()
                    .WriteTo.MariaDB(connString, autoCreateTable: true)
                    .WriteTo.Sink(host.Services.GetRequiredService<SignalRLoggerSeriLogSink>())
                    .WriteTo.Console()
                    .WriteTo.File(formatter,
                        Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(logPathFiles, $"{DateTime.Now.ToString("yyyyMMdd")}-SBSLETL.log")))
                    .CreateLogger();

                ChangeServiceStartParams();
            }
            catch (Exception ex)
            {
                var eventLog = new EventLog
                {
                    Source = "SBSL ETL Service"
                };
                eventLog.WriteEntry($"SBSL ETL Service Startup Log - {ex.Message}", EventLogEntryType.Error);

                Environment.Exit(1);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog((context, services, config) =>
                {
                    //signal_R
                    var signalRLogSink = services.GetRequiredService<SignalRLoggerSeriLogSink>();
                    _ = config.WriteTo.Sink(signalRLogSink);

                    //Postgres
                    var connString = services.GetService<IConfiguration>().GetConnectionString("DefaultConnection");
                    _ = config.WriteTo.MariaDB(connString, autoCreateTable: true);

                    //file
                    var logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "SBSL_ETL", "logs");
                    var logPathFiles = Path.Combine(logsFolder, "log_files");
                    var formatter = new MessageTemplateTextFormatter(
                        "${Timestamp} [{Level}] {Message:l}{NewLine:l}{Exception:l}", CultureInfo.CurrentCulture);
                    _ = config.WriteTo.File(formatter,
                        Path.Combine(Directory.GetCurrentDirectory(),
                            Path.Combine(logPathFiles, $"{DateTime.Now.ToString("yyyyMMdd")}-SBSLETL.log")));
                })
                .ConfigureWebHostDefaults(webBuilder => { _ = webBuilder.UseStartup<Startup>(); });
        }
    }
}
