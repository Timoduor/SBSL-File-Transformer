using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Camt053;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
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
        EmailSender _emailSender;
        volatile bool _isRunning = false;

        public Camt053ConverterJob(ILogger<Camt053ConverterJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
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
                                try
                                {
                                    camtConverter.ProcessCamtFile(file);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);
                                    EmailHelpers.SendEmails(dbContext, "Error in CAMT file conversion" ,$"Problem with XML file {file} \n\n {ex.Message}", new string[] { file }, _emailSender);
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
            await _timer.DisposeAsync();
        }
    }
}
