using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Jobs.Converters;
using SbslFileTransformer.Infrastructure.Jobs.Extractors;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Sftp;
using Serilog;
using System.IO;

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


            services.AddHostedService<SftpIndependentJob>();
            services.AddHostedService<ScheduledReporterJob>();
            services.AddHostedService<MtBalanceExtractorJob>();
            services.AddHostedService<AuxilliaryProcessesJob>();
            services.AddHostedService<GLBalanceExtractorJob>();
            services.AddHostedService<Camt053ConverterJob>();
            services.AddHostedService<CdmConverterJob>();
            services.AddHostedService<KenSwitchConverterJob>();
            services.AddHostedService<MasterCardConverterJob>();
            services.AddHostedService<MpesaNewLineCharRemoverJob>();
            services.AddHostedService<EpinConverterJob>();
            services.AddHostedService<EP75ConverterJob>();
            services.AddHostedService<CrdbPdfToMTFileJob>();
            services.AddHostedService<BnrConverterJob>();
            services.AddHostedService<MpesaBalanceExtractorJob>();
            services.AddHostedService<MpesaB2CnC2BConverterJob>();

            services.AddHostedService<CamtToMultiCurrJob>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
