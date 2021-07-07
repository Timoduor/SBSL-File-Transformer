using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Jobs.Converters;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Tanzania;
using SbslFileTransformer.Infrastructure.Jobs.Extractors;
using SbslFileTransformer.Infrastructure.Jobs.Others;
using SbslFileTransformer.Infrastructure.Jobs.Reporting;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Sftp;
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection")));

            var keyStore = Path.Combine(Directory.GetCurrentDirectory(), "keys");

            Directory.CreateDirectory(keyStore);

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyStore));

            IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

            services.AddSingleton(physicalProvider);

            services.AddControllersWithViews();

            services.AddRazorPages();
#if DEBUG
            services.AddRazorPages().AddRazorRuntimeCompilation();
#endif

            services.AddTransient<SftpManager>();
            services.AddTransient<EncryptionManager>();
            services.AddTransient<EmailSender>();

            //SPRINT 1
            services.AddHostedService<SftpIndependentJob>();
            services.AddHostedService<ScheduledReporterJob>();
            services.AddHostedService<MtBalanceExtractorJob>();
            services.AddHostedService<AuxilliaryProcessesJob>();
            services.AddHostedService<GLBalanceExtractorJob>();

            //SPRINT 2
            services.AddHostedService<Camt053ConverterJob>();
            services.AddHostedService<CdmConverterJob>();
            services.AddHostedService<KenSwitchConverterJob>();
            services.AddHostedService<MasterCardConverterJob>();
            services.AddHostedService<MpesaNewLineCharRemoverJob>();
            services.AddHostedService<EpinConverterJob>();
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

            services.AddHostedService<SuspenseTachFileConverterJob>();
            services.AddHostedService<SpennControlBalanceExtractorJob>();
            services.AddHostedService<SelcomDisbursementConverterJob>();


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

            services.AddHostedService<TZ_ATMJournalConverterjob>();
            services.AddHostedService<ATMjournalConverterJob>();
            services.AddHostedService<Tz_Blotter_filesjob>();
            services.AddHostedService<RSwitchConverterJob>();

            services.AddHostedService<FileNetworkCopyJob>();
            services.AddHostedService<ImsBalanceExtractorJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider,
            ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseSerilogRequestLogging();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            ApplicationSeeding.CreateDatabase(serviceProvider, logger).Wait();
        }
    }
}