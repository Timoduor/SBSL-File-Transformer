using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.CDM;
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
    public class CdmConverterJob : ConverterJobBase<CdmConverterJob>, IHostedService
    {
        public CdmConverterJob(ILogger<CdmConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender, JobDisplayManager jobManager)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
            _jobManager = jobManager;
        }

        protected override string JobName { get; set; } = nameof(CdmConverterJob);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting CDM Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertCdmFile(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertCdmFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running CDM converter job");

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

                    List<Configuration> configurations = await dbContext.Configurations.ToListAsync();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.xls", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.xls", options));

                    files.AddRange(Directory.GetFiles(sbFolder, "*.xlsx", options));

                    files.AddRange(Directory.GetFiles(sbFolder, "*.xlsx", options));

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    CurrentJobStatus.Status = JobState.Running;
                    _jobManager.SetJobStatus(JobName, CurrentJobStatus);

                    int count = 0;
                    int total = files.Count;

                    foreach (string file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME CAMT053 SOMEWHERE IN IT
                        if (file.ToLower().Contains("cdm") || file.ToLower().Contains("cash") &&
                            file.ToLower().Contains("deposit") && file.ToLower().Contains("machine"))
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    if (Entity == "IMRW")
                                    {
                                        CdmConverterRwanda cdmConverter = new CdmConverterRwanda();
                                        cdmConverter.ConvertFile(file);
                                    }

                                    if (Entity == "IMKE")
                                    {
                                        CdmFileConverter cdmConverter = new CdmFileConverter();
                                        cdmConverter.ConvertFile(file);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    if (Entity == "IMRW") fileToProcess.ConvertedBy = nameof(CdmConverterRwanda);
                                    if (Entity == "IMKE") fileToProcess.ConvertedBy = nameof(CdmFileConverter);

                                    updatedFiles.Add(fileToProcess);
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