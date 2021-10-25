using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Kenya;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class Mt300sTZConverterJob : ConverterJobBase<Mt300sTZConverterJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(Mt300sTZConverterJob);
        public Mt300sTZConverterJob(ILogger<Mt300sTZConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MT300s Converter TZ Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await MT300sConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task MT300sConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running MT300s Converter TZ job");

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

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".txt"))
                        .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".txt")));

                    var mt300Converter = new Mt300KEConverter();
                    var mt320Converter = new Mt320KEConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    var updatedFiles = new List<SftpUploadedFile>();

                    foreach (var file in files)
                    {
                        //SPECIFY FOLDER and file extension above PENDING

                        if ((file.ToLower().Contains("mt300") || file.ToLower().Contains("mt320"))
                            && file.ToLower().Contains("fx_confirmations") && file.ToLower().Contains("imtz") &&
                            !file.Contains("Conv"))
                        {
                            var fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    if (file.ToLower().Contains("mt300")) mt300Converter.ConvertFile(file, entity: "IMTZ");

                                    if (file.ToLower().Contains("mt320")) mt320Converter.ConvertFile(file, entity: "IMTZ");
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(Mt300sTZConverterJob));
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