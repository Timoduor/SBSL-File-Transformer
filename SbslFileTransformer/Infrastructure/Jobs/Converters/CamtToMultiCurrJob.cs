using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class CamtToMultiCurrJob : IHostedService
    {
        private Timer _timer;
        private readonly ILogger<PdfToMTFileJob> _logger;
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly EmailSender _emailSender;
        volatile bool _isRunning;

        public CamtToMultiCurrJob(ILogger<PdfToMTFileJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting PDF To MT File Converter Job");

            _timer = new Timer(state => ConvertCamtToMultiCurrFile(), null, TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void ConvertCamtToMultiCurrFile()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

                _logger.LogInformation("Running PDF To MT File converter job");

                string prodFolder;
                string sbFolder;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                    //var cdmConverter = new CdmFileConverter();

                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME CAMT053 SOMEWHERE IN IT
                        if (file.ToLower().Contains("camt053") && file.ToLower().Contains("bals"))
                        {
                            var fileToProcess = dbContext.UploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    //cdmConverter.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);

                                    EmailHelpers.SendEmails(dbContext, "Problem Converting CDM files", $"{file} \n\n {ex.Message}", new string[] { file }, _emailSender);
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
