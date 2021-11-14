using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class CDMBalanceExtractorJob : ConverterJobBase<CDMBalanceExtractorJob>, IHostedService
    {
        public CDMBalanceExtractorJob(ILogger<CDMBalanceExtractorJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender, JobDisplayManager jobManager)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
            _jobManager = jobManager;
        }

        protected override string JobName { get; set; } = nameof(CDMBalanceExtractorJob);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MPesa Balance Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await CDMFileBalanceExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task CDMFileBalanceExtractor()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running CDM Balance Extractor job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    CurrentJobStatus = _jobManager.GetJobStatus(JobName);

                    if (CurrentJobStatus == null)
                    {
                        CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Starting };

                        _jobManager.SetJobStatus(JobName, CurrentJobStatus);
                    }

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options)
                        .Where(f => f.ToLower().EndsWith(".csv") || f.ToLower().EndsWith(".xlsx")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f =>
                        f.ToLower().EndsWith(".csv") || f.ToLower().EndsWith(".xlsx")));

                    CDMBalanceExtractor mpesaConverter = new CDMBalanceExtractor
                    {
                        ServiceScopeFactory = _serviceScopeFactory,
                        Entity = Entity
                    };

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    CurrentJobStatus.Status = JobState.Running;
                    _jobManager.SetJobStatus(JobName, CurrentJobStatus);

                    int count = 0;
                    int total = files.Count;

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("cdm") && file.ToLower().Contains("bal") ||
                            file.ToLower().Contains("cash")
                            && file.ToLower().Contains("deposit") && file.ToLower().Contains("machine") &&
                            file.ToLower().Contains("bal"))
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    bool isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                        false.ToString());

                                    string rootFolder = isProd ? prodFolder : sbFolder;

                                    await mpesaConverter.ConvertFile(file, rootFolder, Entity);
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(CDMBalanceExtractor));
                                }
                        }
                        CurrentJobStatus.ProgressMessage = $"Currently processing {file}... {count} of {total}";
                        CurrentJobStatus.SetProgress(count, total);
                        _jobManager.SetJobStatus(JobName, CurrentJobStatus);
                    }
                    await SaveProcessedFilesStatuses(dbContext, updatedFiles);

                    CurrentJobStatus.Status = JobState.Completed;
                    _jobManager.SetJobStatus(JobName, CurrentJobStatus);
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