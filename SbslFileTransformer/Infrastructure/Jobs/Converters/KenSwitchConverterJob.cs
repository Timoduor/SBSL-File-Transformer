using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.KenSwitch;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class KenSwitchConverterJob : IHostedService
    {
        private Timer _timer;
        IServiceScopeFactory _serviceScopeFactory;
        ILogger<KenSwitchConverterJob> _logger;
        EmailSender _emailSender;
        volatile bool _isRunning;

        public KenSwitchConverterJob(ILogger<KenSwitchConverterJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting KenSwitch Converter Job");

            _timer = new Timer(state => ConvertKenSwitchPdfs(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void ConvertKenSwitchPdfs()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

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

                    var files = Directory.GetFiles(prodFolder, "*.pdf", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.pdf", options));

                    var converter = new KenSwitchConverter();

                    foreach (var file in files)
                    {
                        if (file.ToLower().Contains("kenswitch"))
                        {
                            var fileToProcess = dbContext.UploadedFiles.Where(f => f.FilePath.ToLower() == file.ToLower()).FirstOrDefault();

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    converter.ConverterKenSwitchFile(file);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);

                                    EmailHelpers.SendEmails(dbContext, "Problem Converting KenSwitch files", $"{file} \n\n {ex.Message}", new string[] { file }, _emailSender);
                                }

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
            finally
            {
                _isRunning = false;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping KS converter job");

            await _timer.DisposeAsync();
        }
    }
}
