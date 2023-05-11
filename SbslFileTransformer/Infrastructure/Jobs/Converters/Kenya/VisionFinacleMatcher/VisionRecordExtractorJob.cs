extern alias MySqlDataAlias;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class VisionRecordExtractorJob : ConverterJobBase<VisionRecordExtractorJob>, IHostedService
    {
        int batchSize = 100000;

        protected override string JobName { get; set; } = nameof(VisionRecordExtractorJob);
        public VisionRecordExtractorJob(ILogger<VisionRecordExtractorJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender, JobDisplayManager jobManager)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
            this._jobManager = jobManager;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Vision Record Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.VisionRecordExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task VisionRecordExtractor()
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

                    this.CurrentJobStatus.Status = JobState.Running;
                    this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx") || f.ToLower().EndsWith(".csv"))
                       .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx") || f.ToLower().EndsWith(".csv")));

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    int count = 0;
                    int total = files.Count;

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("cards") && file.ToLower().Contains("imke"))
                        {
                            VisionRecordType visionRecordType = VisionCommonHelpers.GetVisionRecordType(file);

                            if (visionRecordType == VisionRecordType.None)
                                continue;

                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    Stopwatch recordReader = Stopwatch.StartNew();

                                    List<VisionRecordBase> records = await this.GetRecordsFromVisionFile(file);

                                    this._logger.LogInformation($"It took {recordReader.ElapsedMilliseconds / 1000} seconds to READ {records.Count} vision records from file {file}");

                                    recordReader.Restart();

                                    List<Task> tasks = new List<Task>();

                                    foreach (IEnumerable<VisionRecordBase> batch in records.Batch(batchSize))
                                    {
                                        tasks.Add(this.InsertRecordsToDb(batch.ToList()));
                                    }

                                    await Task.WhenAll(tasks);

                                    this._logger.LogInformation($"It took {recordReader.ElapsedMilliseconds / 1000} seconds to SAVE the {records.Count} records from file {file} to the database");
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(VisionRecordExtractorJob));
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

        private async Task<List<VisionRecordBase>> GetRecordsFromVisionFile(string glFile)
        {
            this._logger.LogInformation($"Started Vision Record Extraction for {glFile.ToUpper()}");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<VisionRecordBase> glCmsRecs = new List<VisionRecordBase>();

            Stopwatch sw = Stopwatch.StartNew();

            using (FileStream stream = File.Open(glFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(glFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                while (reader.Read())
                {
                    if (!DateTime.TryParse(reader.GetValue(0)?.ToString(), out DateTime result))
                        continue;

                    AddRecordsToList(reader, result, glCmsRecs, glFile);
                }

                await stream.FlushAsync();

                stream.Close();
            }

            this._logger.LogInformation($"Finished Vision Record Extraction for {glFile.ToUpper()} in {sw.ElapsedMilliseconds} milliseconds");

            return glCmsRecs;
        }

        private void AddRecordsToList(IExcelDataReader reader, DateTime extractedDate, List<VisionRecordBase> glCmsRecs, string glFile)
        {
            var glRec = new VisionRecordCollection
            {
                BankingDate = extractedDate,
                TransDetails = reader.GetValue(1)?.ToString(),
                TransID = reader.GetValue(2)?.ToString(),
                ReferenceNumber = reader.GetValue(3)?.ToString(),
                GLTransCode = reader.GetValue(4)?.ToString(),
                CardNumber = reader.GetValue(5)?.ToString(),
                CreditAmount = Convert.ToDouble(reader.GetValue(6)?.ToString()),
                DebitAmount = Convert.ToDouble(reader.GetValue(7)?.ToString()),
                CustomerName = reader.GetValue(8)?.ToString(),
                ContractNumber = reader.GetValue(9)?.ToString(),
                AccountNumber = reader.GetValue(10)?.ToString(),
                ChequeNo = reader.GetValue(11)?.ToString(),
                AuthorizationCode = reader.GetValue(12)?.ToString(),
                PrimaryEntryIDT = reader.GetValue(13)?.ToString(),
                FileName = glFile,
                DateExtracted = DateTime.Now
            };

            glCmsRecs.Add(glRec);
        }

        private async Task InsertRecordsToDb(List<VisionRecordBase> records)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                using (ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    await dbContext.VisionRecordCollections
                        .AddRangeAsync(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordCollection>(records));

                    await dbContext.SaveChangesAsync();
                }

                this._logger.LogInformation($"Added {records.Count} vision records to DbContext in {sw.ElapsedMilliseconds} milliseconds");
            }
        }
    }
}
