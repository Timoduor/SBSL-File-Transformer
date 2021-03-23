using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.CDM;
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
    public class CdmConverterJob : ConverterJobBase, IHostedService
    {
        private Timer _timer;
        private ILogger<CdmConverterJob> _logger;
        IServiceScopeFactory _serviceScopeFactory;
        EmailSender _emailSender;
        private static SemaphoreSlim _semaphore;

        public CdmConverterJob(ILogger<CdmConverterJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting CDM Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertCdmFile(), null, TimeSpan.FromSeconds(new Random().Next(5, 20)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertCdmFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running CDM converter job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = await dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToListAsync();

                    Entity = dbContext.Configurations.FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.xls", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.xls", options));



                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME CAMT053 SOMEWHERE IN IT
                        if (file.ToLower().Contains("cdm") || (file.ToLower().Contains("cash") && file.ToLower().Contains("deposit") && file.ToLower().Contains("machine")))
                        {
                            var fileToProcess = await dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    if (Entity == "IMRW")
                                    {
                                        var cdmConverter = new CdmConverterRwanda();
                                        cdmConverter.ConvertFile(file);
                                    }
                                    if (Entity == "IMKE")
                                    {
                                        var cdmConverter = new CdmFileConverter();
                                        cdmConverter.ConvertFile(file);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting CDM files", $"{file} \n\n {ex.Message}", new string[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();
                                }

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
                _semaphore.Release();
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }
    }
}
