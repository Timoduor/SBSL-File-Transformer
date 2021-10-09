using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class FxposfcdailyJob : ConverterJobBase<FxposfcdailyJob>, IHostedService
    {

        public FxposfcdailyJob(ILogger<FxposfcdailyJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Fxposfcdaily  Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await FxposfcdailyConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task FxposfcdailyConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Fxposfcdaily Converter RW job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx")));
                    var fc_Converter = new Fc_dailyConverter();
                    var isProd = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ?? false.ToString());
                    var rootFolder = isProd ? prodFolder : sbFolder;

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    var updatedFiles = new List<SftpUploadedFile>();

                    foreach (var file in files)
                    {
                        //SPECIFY FOLDER and file extension above PENDING
                        //imrw/treasury/fx_pos/fx_pos_fc_daily
                        if (file.ToLower().Contains("imrw") && file.ToLower().Contains("treasury") && file.ToLower().Contains("fx_pos") && file.ToLower().Contains("fx_pos_fc_daily"))//&& !file.ToLower().Contains("conv")
                        {
                            var fileToProcess = uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    fc_Converter.ConvertFile(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, typeof(Fc_dailyConverter));
                                }
                        }
                    }
                    await SaveProcessedFilesStatuses(dbContext, updatedFiles);
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

    }
}
