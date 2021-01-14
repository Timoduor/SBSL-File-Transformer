using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
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
    public class MasterCardConverterJob : IHostedService
    {
        private ILogger<MasterCardConverterJob> _logger;
        IServiceScopeFactory _serviceScopeFactory;
        EmailSender _emailSender;
        volatile bool _isRunning;
        Timer _timer;

        public MasterCardConverterJob(ILogger<MasterCardConverterJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MasterCard Converter Job");

            _timer = new Timer(state => ConvertMasterCardFile(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void ConvertMasterCardFile()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

                _logger.LogInformation("Running MasterCard converter job");

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

                    var files = Directory.GetFiles(prodFolder, "*.a024", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.a024", options));

                    var masterCardConverter = new MasterCardConverter();

                    foreach (var file in files)
                    {
                        if (file.ToLower().Contains("mastercard"))
                        {
                            var fileToProcess = dbContext.UploadedFiles.Where(f => f.FilePath.ToLower() == file.ToLower()).FirstOrDefault();

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    masterCardConverter.ConvertFile(file);
                                }
                                catch(Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);

                                    EmailHelpers.SendEmails(dbContext, "Problem Converting MasterCard files", $"{file} \n\n {ex.Message}", new string[] { file }, _emailSender);
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
