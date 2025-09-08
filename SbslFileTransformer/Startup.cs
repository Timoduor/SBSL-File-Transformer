using System;
using System.IO;
using System.Threading;

using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SbslFileTransformer.Data;
using SbslFileTransformer.Hubs;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Jobs.Converters;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Rwanda;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Rwanda.BNR;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Tanzania;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Uganda;
using SbslFileTransformer.Infrastructure.Jobs.Extractors;
using SbslFileTransformer.Infrastructure.Jobs.Extractors.Uganda;
using SbslFileTransformer.Infrastructure.Jobs.Others;
using SbslFileTransformer.Infrastructure.Jobs.Reporting;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers.Interfaces;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Infrastructure.SignalRLogging;
using SbslFileTransformer.Models.Enums;

using Serilog;

namespace SbslFileTransformer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString
                    , ServerVersion.AutoDetect(connectionString), opts =>
                    {
                        opts.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                    })
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
            );

            services.AddSignalR();

            services.AddSingleton<SignalRLoggingQueue>();
            services.AddSingleton<SignalRLoggerSeriLogSink>();
            services.AddHostedService<SignalRLoggingBackgroundService>();

            services.AddHealthChecks();
            services.AddMiniProfiler(options => options.RouteBasePath = "/profiler").AddEntityFramework();

            var keyStore = Path.Combine(Directory.GetCurrentDirectory(), "keys");

            Directory.CreateDirectory(keyStore);

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyStore));

            IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

            services.AddSingleton(physicalProvider);

            services.AddControllersWithViews();

            services.AddMemoryCache();

            services.AddRazorPages();
#if DEBUG
            services.AddRazorPages().AddRazorRuntimeCompilation();
#endif

            services.AddHttpClient("BlackLine", c =>
            {
                c.Timeout = TimeSpan.FromSeconds(25);
            });

            services.AddTransient<JobDisplayManager>();
            services.AddTransient<SftpManager>();
            services.AddTransient<EncryptionManager>();
            services.AddTransient<EmailSender>();

            //Reporting
            services.AddTransient<IReportsDownloader, ReportListDownloader>();
            services.AddTransient<IReportProcessor, ReportProcessor>();
            services.AddHostedService<ReportEngineJob>();

            //SPRINT 1
            services.AddHostedService<SftpIndependentJob>();

            services.AddHostedService<MtBalanceExtractorJob>();
            services.AddHostedService<AuxilliaryProcessesJob>();
            services.AddHostedService<GLBalanceExtractorJob>();

            //SPRINT 2
            services.AddHostedService<Camt053ConverterJob>();
            services.AddHostedService<CdmConverterJob>();
            services.AddHostedService<KenSwitchConverterJob>();
            services.AddHostedService<MasterCardConverterJob>();
            services.AddHostedService<MpesaNewLineCharRemoverJob>();
            //services.AddHostedService<EpinConverterJob>();
            services.AddHostedService<EP75ConverterJob>();
            services.AddHostedService<CrdbPdfToMTFileJob>();
            services.AddHostedService<DtbPdfToMTFileJob>();
            services.AddHostedService<BnrConverterJob>();
            services.AddHostedService<MpesaBalanceExtractorJob>();
            services.AddHostedService<MpesaB2CnC2BConverterJob>();
            services.AddHostedService<CDMBalanceExtractorJob>();
            services.AddHostedService<AirtelKenyaBalanceExtractorJob>();
            services.AddHostedService<SelcomBalanceExtractorJob>();
            services.AddHostedService<BnrSettlementConverterJob>();

            //SPRINT 2-2
            services.AddHostedService<BillerUtilBalanceExtractorJob>();
            services.AddHostedService<BillerUtilCleanerJob>();
            services.AddHostedService<AirtelRwandaBalanceExtractorJob>();
            services.AddHostedService<MTNRwandaBalanceExtractorJob>();
            services.AddHostedService<SpennRwandaBalanceExtractorJob>();
            services.AddHostedService<LipaNaMpesaC2BMerchantConverterJob>();
            services.AddHostedService<MTNPushPullRwandaBalanceExtractorJob>();
            services.AddHostedService<DailyElmaOmniConverterJob>();
            services.AddHostedService<WeeklyMonthlyElmaOmniConverterJob>();
            services.AddHostedService<OmniLookupConverterJob>();
            services.AddHostedService<CamtToMultiCurrJob>();

            //SPRINT 3
            services.AddHostedService<SuspenseTachBalanceExtractorJob>();
            services.AddHostedService<SpennControlBalanceExtractorJob>();
            services.AddHostedService<SuspenseTachFileConverterJob>();
            //services.AddHostedService<SelcomDisbursementConverterJob>();


            //SPRINT 4
            services.AddHostedService<WesternUnionSettlementKEConverterJob>();
            services.AddHostedService<WesternUnionActivitiesKEConverterJob>();
            services.AddHostedService<WesternUnionSettlementRWConverterJob>();
            services.AddHostedService<WesternUnionActivitiesRWConverterJob>();
            services.AddHostedService<MoneyGramActivityKEConverterJob>();
            services.AddHostedService<MoneyGramSettlementKEConverterJob>();
            services.AddHostedService<MoneyGramActivityRWConverterJob>();
            services.AddHostedService<MoneyGramSettlementRWConverterJob>();
            services.AddHostedService<AdviceCopeduRWConverterJob>();
            services.AddHostedService<Mt300sKEConverterJob>();

            services.AddHostedService<MT300RWConverterJob>();
            services.AddHostedService<MT320RWConverterJob>();
            services.AddHostedService<Mt300sTZConverterJob>();
            services.AddHostedService<FxRatesTzConverterJob>();

            services.AddHostedService<OUTMT300ConverterJob>();
            services.AddHostedService<OUTMT320ConverterJob>();
            services.AddHostedService<TzAtmJournalConverterJob>();
            services.AddHostedService<ATMjournalConverterJob>();
            services.AddHostedService<Tz_Blotter_filesjob>();
            services.AddHostedService<RSwitchConverterJob>();


            //Sprint 5
            services.AddHostedService<EOD_DealsJob>();
            services.AddHostedService<ATMBalanceExtractorJob>();
            services.AddHostedService<Sumtreasuryfxposjob>();
            services.AddHostedService<FxposfcdailyJob>();
            services.AddHostedService<FxposftdailyJob>();
            services.AddHostedService<TZEpinConverterJob>();
            services.AddHostedService<TZ_EP75ConverterJob>();
            services.AddHostedService<Repo2_ConverterJob>();
            services.AddHostedService<PrepaidAuthrptjob>();
            services.AddHostedService<PrepaidCardbalanceJob>();
            services.AddHostedService<FxPosGLBalJob>();
            services.AddHostedService<TZRepo2_ConverterJob>();
            services.AddHostedService<RW_notonusposvisa_Job>();
            services.AddHostedService<PesaLinkGl2ConverterJob>();
            services.AddHostedService<PesaLinkStatementConverterJob>();
            services.AddHostedService<PesaLinkTransConverterJob>();
            services.AddHostedService<MasterCardRWConverterJob>();

            //Uganda
            services.AddHostedService<AirtelUgandaBalanceExtractorJob>();
            services.AddHostedService<MtnUgandaBalanceExtractorJob>();
            services.AddHostedService<UG_OUTMT300ConverterJob>();
            services.AddHostedService<MoneyGramActivityUGConverterJob>();
            services.AddHostedService<MoneyGramSettlementUGConverterJob>();
            services.AddHostedService<BouSettlementConverterJob>();
            services.AddHostedService<OutwardEftsConverterJob>();
            services.AddHostedService<BouMultiCurrExtractorJob>();


            //Phase 4
            services.AddHostedService<keATMConverterJob>();
            services.AddHostedService<stmtPdfMTFilesjob>();
            services.AddHostedService<keLogbookConverterJob>();
            services.AddHostedService<keDebtorslistConverterJob>();
            services.AddHostedService<Epin06ConverterJob>();
            services.AddHostedService<NCR_JournalsConverterJob>();

            //special scenario jobs
            //services.AddHostedService<RecordMatcherJob>();
            //services.AddHostedService<VisionRecordExtractorJob>();
            services.AddHostedService<MultiCurrBalanceExtractorJob>();


            services.AddHostedService<FileNetworkCopyJob>();
            services.AddHostedService<ImsBalanceExtractorJob>();
            services.AddHostedService<BotWrapRemoverJob>();
            services.AddHostedService<AirtelB2CKenyaBalanceExtractorJob>();
            services.AddHostedService<AirtelC2BKenyaBalanceExtractorJob>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider,
            ILogger<Startup> logger, IHostApplicationLifetime applicationLifetime, IMemoryCache cache)
        {
            //app.UseRequestResponseLoggingMiddleware();

            _ = app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            _ = app.UseMiniProfiler();

            var appShutdownInput = new Tuple<IMemoryCache, ILogger<Startup>>(cache, logger);

            _ = applicationLifetime.ApplicationStopping
                .Register(i => OnAppShutdown((Tuple<IMemoryCache, ILogger<Startup>>)i), appShutdownInput);

            _ = app.UseDeveloperExceptionPage();

            _ = app.UseSerilogRequestLogging(config =>
            {
                config.IncludeQueryInRequestPath = true;
            });

            _ = app.UseExceptionHandler("/Home/Error");

            _ = app.UseHttpsRedirection();
            _ = app.UseStaticFiles();

            _ = app.UseRouting();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
                _ = endpoints.MapRazorPages();

                _ = endpoints.MapHub<LogsHub>("/logsHub");
            });

            ApplicationSeeding.CreateDatabase(serviceProvider, logger).Wait();
        }

        //if file upload job is running wait for completion
        private void OnAppShutdown(Tuple<IMemoryCache, ILogger<Startup>> input)
        {
            //check for job state of upload job in memcache
            //log if job is still running and wait for completion before continuing shutdown
            if (input.Item1.TryGetValue(nameof(SftpIndependentJob), out JobStatus result))
            {
                while (result.Status != JobState.Completed)
                {
                    input.Item2.LogWarning("An important job is still running. Waiting for it to complete...");
                    Thread.Sleep(2000);
                }
            }

            input.Item2.LogInformation("SBSL ETL Application Shutting down...");
        }
    }
}
