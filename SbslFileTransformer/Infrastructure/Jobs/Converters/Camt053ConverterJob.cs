using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Camt053;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class Camt053ConverterJob : IHostedService
    {
        private Timer _timer;
        private ILogger<Camt053ConverterJob> _logger;
        IServiceScopeFactory _serviceScopeFactory;

        public Camt053ConverterJob(ILogger<Camt053ConverterJob> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting CAMT053 Converter Job");

            _timer = new Timer(state => ConvertCamtFile(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void ConvertCamtFile()
        {
            try
            {
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


                    var options = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.xml", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.xml", options));

                    var camtConverter = new Camt053Converter();

                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME CAMT053 SOMEWHERE IN IT
                        if (file.ToLower().Contains("camt053"))
                        {
                            var fileToProcess = dbContext.UploadedFiles.Where(f => f.FilePath.ToLower() == file.ToLower()).FirstOrDefault();

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                camtConverter.ProcessCamtFile(file);

                                //mark the file as already converted
                                fileToProcess.Converted = true;

                                dbContext.Update(fileToProcess);

                                dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }
    }
}
