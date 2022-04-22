using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Others
{
    public class FileNetworkCopyJob : ConverterJobBase<FileNetworkCopyJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(FileNetworkCopyJob);

        public FileNetworkCopyJob(ILogger<FileNetworkCopyJob> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _semaphore = new SemaphoreSlim(1, 1);

            this._logger.LogInformation("Starting network file transfer job");

            this._timer = new Timer(async state => await this.CopyFilesToNetworkPath(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromMinutes(20));

            return Task.CompletedTask;
        }

        private async Task CopyFilesToNetworkPath()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running network file transfer Job");

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    string networkFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                        b.ConfigType == ConfigurationType.Setting && b.Key == "NetworkFolder"))?.Value;

                    string localFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                        b.ConfigType == ConfigurationType.Setting && b.Key == "LocalFolder"))?.Value;

                    if (string.IsNullOrEmpty(networkFolder) || string.IsNullOrEmpty(localFolder)) return;

                    if (Directory.Exists(localFolder) && Directory.Exists(networkFolder))
                    {
                        EnumerationOptions enumOptions = new EnumerationOptions
                        { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                        foreach (string file in Directory.GetFiles(localFolder, "*.*", enumOptions))
                        {
                            string relativePath = Path.GetRelativePath(localFolder, file);
                            string destination = Path.Combine(networkFolder, relativePath);

                            if (!File.Exists(destination))
                            {
                                string directory = Path.GetDirectoryName(destination);

                                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                                File.Copy(file, destination, true);
                            }
                        }
                    }
                    else
                    {
                        this._logger.LogWarning("Missing or mis-configured local or network path!");
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}