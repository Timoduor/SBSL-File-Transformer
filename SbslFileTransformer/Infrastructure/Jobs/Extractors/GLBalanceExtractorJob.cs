using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Extractors
{
    public class GLBalanceExtractorJob : IHostedService
    {
        private Timer _timer;
        private ILogger<GLBalanceExtractorJob> _logger;
        private IServiceScopeFactory _serviceScopeFactory;
        static SemaphoreSlim _semaphore;

        public GLBalanceExtractorJob(IServiceScopeFactory serviceScopeFactory, ILogger<GLBalanceExtractorJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await ExtractGLBalances(), null, TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        private async Task ExtractGLBalances()
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

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

                    Entity = dbContext.Configurations.FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;
                }

                var options = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                var files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                var converter = new BalanceFileConverter(_logger, _serviceScopeFactory, Entity);

                foreach (var file in files)
                {
                    if ((file.ToLower().Contains("nostro_balance".ToLower()) || file.ToLower().Contains("bnr_balance".ToLower())
                        || file.ToLower().Contains("b2w_balance".ToLower()) || file.ToLower().Contains("selcom_balance".ToLower())
                        || file.ToLower().Contains("mb_balance".ToLower()))
                        && Path.GetExtension(file.ToLower()) != ".txt")
                    {
                        try
                        {
                            if (file.ToLower().Contains("mb_balance".ToLower()) || file.ToLower().Contains("selcom_balance".ToLower())
                                || file.ToLower().Contains("b2w_balance".ToLower()))
                            {
                                await converter.Execute(file, "Mobile banking");
                            }
                            else
                            {
                                await converter.Execute(file);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }
                    }
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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }
    }
}
