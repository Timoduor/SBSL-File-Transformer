using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Others
{
    public class FileNetworkCopyJob : IHostedService
    {
        ILogger<FileNetworkCopyJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private static SemaphoreSlim _semaphore;
        private static Timer _timer;

        public FileNetworkCopyJob(ILogger<FileNetworkCopyJob> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _semaphore = new SemaphoreSlim(1, 1);

            _logger.LogInformation("Starting network file transfer job");

            _timer = new Timer(async (state) => await CopyFilesToNetworkPath(), null, TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task CopyFilesToNetworkPath()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running network file transfer Job");

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var networkFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b => b.ConfigType == Models.Enums.ConfigurationType.Setting && b.Key == "NetworkFolder"))?.Value;

                    var localFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b => b.ConfigType == Models.Enums.ConfigurationType.Setting && b.Key == "LocalFolder"))?.Value;

                    if(string.IsNullOrEmpty(networkFolder) || string.IsNullOrEmpty(localFolder))
                    {
                        return;
                    }

                    if (Directory.Exists(localFolder) && Directory.Exists(networkFolder))
                    {
                        var enumOptions = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                        foreach (var file in Directory.GetFiles(localFolder, "*.*", enumOptions))
                        {
                            var relativePath = Path.GetRelativePath(localFolder, file);
                            var destination = Path.Combine(networkFolder, relativePath);

                            if (!File.Exists(destination))
                            {
                                var directory = Path.GetDirectoryName(destination);

                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                File.Copy(file, destination, true);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Missing or mis-configured local or network path!");
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Copy files to network folder service stopped!");

            _timer.Dispose();

            return Task.CompletedTask;
        }
    }
}
