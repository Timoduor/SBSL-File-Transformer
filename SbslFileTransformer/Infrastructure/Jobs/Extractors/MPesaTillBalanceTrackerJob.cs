using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.BalanceExtractors.Kenya;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;

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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await UpdateMPesaBalances(), null,
                TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromMinutes(30));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }

        private async Task UpdateMPesaBalances()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running GL Balance Extractor Job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                        .ToList();

                    Entity = dbContext.Configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;
                }

                var options = new EnumerationOptions
                    {RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive};

                var files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                var converter = new MpesaTillBalanceTracker();

                foreach (var file in files)
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