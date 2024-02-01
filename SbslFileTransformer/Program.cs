using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SbslFileTransformer.Infrastructure.ServiceManager;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Display;

namespace SbslFileTransformer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AddLogging();

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

        private static void AddLogging()
        {
            string logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SBSL_ETL", "logs");
            string logPathFiles = Path.Combine(logsFolder, "log_files");
            string logPathSqlite = Path.Combine(logsFolder, "log_sqlite");

            Directory.CreateDirectory(logPathFiles);
            Directory.CreateDirectory(logPathSqlite);

            try
            {
                MessageTemplateTextFormatter formatter = new MessageTemplateTextFormatter(
                    "${Timestamp} [{Level}] {Message:l}{NewLine:l}{Exception:l}", CultureInfo.CurrentCulture);

                Log.Logger = new LoggerConfiguration()
#if DEBUG
                    .MinimumLevel.Information()
#else
                .MinimumLevel.Information()
#endif
                    .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore"))
                    .Enrich.FromLogContext()
                    //.WriteTo.SQLite(Path.Combine(logPathSqlite, "sbsletl_logs.db"),
                    //    retentionPeriod: TimeSpan.FromDays(7), rollOver: false, maxDatabaseSize: 20480)
                    .WriteTo.Console()
                    .WriteTo.File(formatter,
                        Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(logPathFiles, $"{DateTime.Now.ToString("yyyyMMdd")}-SBSLETL.log")))
                    .CreateLogger();

                ChangeServiceStartParams();
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);
                Directory.CreateDirectory(Path.Combine(logPathSqlite, "Old"));
                //move corrupt sqlite log file to old files
                File.Move(Path.Combine(logPathSqlite, "sbsletl_logs.db"),
                    Path.Combine(logPathSqlite, "Old",
                        $"sbsletl_logs_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.db"));
                //log the incident
                File.AppendAllText(Path.Combine(logPathSqlite, "SQLite_Problems.txt"),
                    $"Sqlite Log file Delete due to corruption {DateTime.Now:yyyy_MM_dd_HH_mm_ss}\n\n");
                //restart the service
                EventLog eventLog = new EventLog
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
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
