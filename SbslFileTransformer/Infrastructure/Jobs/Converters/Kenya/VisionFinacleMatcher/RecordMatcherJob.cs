using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Kenya;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class RecordMatcherJob : ConverterJobBase<RecordMatcherJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(RecordMatcherJob);
        public RecordMatcherJob(ILogger<RecordMatcherJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender, JobDisplayManager jobManager)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
            this._jobManager = jobManager;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Record Matcher Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.RecordMatcherExtractorConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task RecordMatcherExtractorConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running Record Matcher Extractor job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    this.CurrentJobStatus = this._jobManager.GetJobStatus(JobName);

                    if (this.CurrentJobStatus == null)
                    {
                        this.CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Running };

                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    }

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv"))
                       .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")));

                    VisionRecordMatcher mpesaConverter = new VisionRecordMatcher(dbContext);

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    this.CurrentJobStatus.Status = JobState.Running;
                    this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);

                    int count = 0;
                    int total = files.Count;

                    foreach (string file in files)
                    {
                        if (
                            file.ToLower().Contains("cards") && file.ToLower().Contains("imke") && 
                            ((file.ToLower().Contains("credit_card") && file.ToLower().Contains("collections_gl")) || 
                             ((file.ToLower().Contains("debtors") || file.ToLower().Contains("credit_sett")) && file.ToLower().Contains("finacle")))
                           )
                        {
                            VisionRecordType visionRecordType = VisionCommonHelpers.GetVisionRecordType(file);

                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f =>
                                     f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null)// not checked for processed because new records might be added that match
                            {

                                string path = Path.GetDirectoryName(file);
                                string outputPath = Path.Combine(Path.GetFullPath(Path.Combine(path, @"..\")), "Col_Conv");

                                Directory.CreateDirectory(outputPath);

                                try
                                {
                                    await mpesaConverter.MatchFiles(file, outputPath, visionRecordType);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(VisionRecordMatcher));
                                }
                            }
                        }
                        this.CurrentJobStatus.ProgressMessage = $"Currently processing {file}... {count} of {total}";
                        this.CurrentJobStatus.SetProgress(count, total);
                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    }
                    await this.SaveProcessedFilesStatuses(dbContext, updatedFiles);

                    this.CurrentJobStatus.Status = JobState.Completed;
                    this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);

                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
