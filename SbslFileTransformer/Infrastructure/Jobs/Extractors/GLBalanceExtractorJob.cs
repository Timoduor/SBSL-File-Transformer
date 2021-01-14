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
        volatile bool _isRunning;

        public GLBalanceExtractorJob(IServiceScopeFactory serviceScopeFactory, ILogger<GLBalanceExtractorJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(state => ExtractGLBalances(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private void ExtractGLBalances()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

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
                    if (file.ToLower().Contains("Nostro_Balance".ToLower()) && Path.GetExtension(file.ToLower()) != ".txt")
                    {
                        try
                        {
                            converter.Execute(file);
                        }
                        catch(Exception ex)
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
                _isRunning = false;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }
    }
}
