using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
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
    public class MpesaNewLineCharRemoverJob : IHostedService
    {
        Timer _timer;
        ILogger<MpesaNewLineCharRemoverJob> _logger;
        IServiceScopeFactory _serviceScopeFactory;
        volatile bool _isRunning;
        EmailSender _emailSender;

        public MpesaNewLineCharRemoverJob(ILogger<MpesaNewLineCharRemoverJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting NewLine Remover Job");

            _timer = new Timer(state => ConvertC2BFile(), null, TimeSpan.FromSeconds(new Random().Next(10, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void ConvertC2BFile()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

                _logger.LogInformation("Running CDM converter job");

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

                    var files = Directory.GetFiles(prodFolder, "*.xlsx", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.xlsx", options));

                    var cdmConverter = new MpesaCharRemover();

                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME CAMT053 SOMEWHERE IN IT
                        if (file.ToLower().Contains("mpesa") || file.ToLower().Contains("c2b"))
                        {
                            var fileToProcess = dbContext.UploadedFiles.Where(f => f.FilePath.ToLower() == file.ToLower()).FirstOrDefault();

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    cdmConverter.FindAndReplaceOccurrences(file, "\n", " ");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);

                                    EmailHelpers.SendEmails(dbContext, "Problem Converting mpesa C2B file", $"{file} \n\n {ex.Message}", new string[] { file }, _emailSender);
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
