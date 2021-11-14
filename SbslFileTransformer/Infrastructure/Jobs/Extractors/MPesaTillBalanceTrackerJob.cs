using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.BalanceExtractors.Kenya;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Extractors
{
    public class MPesaTillBalanceTrackerJob : ConverterJobBase<MPesaTillBalanceTrackerJob>, IHostedService
    {
        public MPesaTillBalanceTrackerJob(IServiceScopeFactory serviceScopeFactory,
            ILogger<MPesaTillBalanceTrackerJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override string JobName { get; set; } = nameof(ImsBalanceExtractorJob);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await UpdateMPesaBalances(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(30));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        private async Task UpdateMPesaBalances()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running GL Balance Extractor Job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    System.Collections.Generic.List<Models.Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;
                }

                EnumerationOptions options = new EnumerationOptions
                { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                System.Collections.Generic.List<string> files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                MpesaTillBalanceTracker converter = new MpesaTillBalanceTracker();

                foreach (string file in files)
                {
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}