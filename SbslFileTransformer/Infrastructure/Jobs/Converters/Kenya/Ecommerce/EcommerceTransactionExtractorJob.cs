using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.Ecommerce.Models;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.Ecommerce
{
    /// <summary>
    /// Extracts the trasactions for Ecommerce and inserts them into the database for balance comparison
    /// /// </summary>
    public class EcommerceTransactionExtractorJob : ConverterJobBase<EcommerceTransactionExtractorJob>, IHostedService
    {
        int batchSize = 50000;

        protected override string JobName { get; set; } = nameof(EcommerceTransactionExtractorJob);

        private IMemoryCache _memoryCache;

        public EcommerceTransactionExtractorJob(ILogger<EcommerceTransactionExtractorJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender, JobDisplayManager jobManager, IMemoryCache memoryCache)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
            this._jobManager = jobManager;
            this._memoryCache = memoryCache;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Ecommerce Transaction Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.EcommerceTransactionExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(30, 100)), TimeSpan.FromMinutes(15));

            return Task.CompletedTask;
        }

        private async Task EcommerceTransactionExtractor()
        {
            try
            {
                await _semaphore.WaitAsync();

                //_memoryCache.Set("EcommerceExtractorLock", true);

                this._logger.LogInformation("Running Ecommerce Record Extractor job");

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

                    var orderedFiles = files.OrderBy(f => new FileInfo(f).Length).ToList();

                    foreach (string file in orderedFiles)
                    {
                        if (file.ToLower().Contains("cards", StringComparison.OrdinalIgnoreCase)
                            && file.ToLower().Contains("imke", StringComparison.OrdinalIgnoreCase)
                            && file.ToLower().Contains("e-commerce", StringComparison.OrdinalIgnoreCase))
                        {
                            VisionRecordType visionRecordType = VisionCommonHelpers.GetVisionRecordType(file);

                            if (visionRecordType == VisionRecordType.None)
                                continue;

                            SftpUploadedFile fileToProcess =
                            uploadedFiles.FirstOrDefault(f => string.Equals(f.FilePath, file, StringComparison.OrdinalIgnoreCase));

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    Stopwatch recordReader = Stopwatch.StartNew();

                                    _logger.LogInformation($"Started processing ECOMMERCE RECORD file: {file.ToUpper()}");

                                    List<EcommerceDbRecord> records = await GetRecordsFromVisionFile(file);

                                    this._logger.LogInformation($"It took {recordReader.ElapsedMilliseconds / 1000} seconds to READ {records.Count} vision records from file {file.ToUpper()}");

                                    recordReader.Restart();

                                    foreach (var batch in records.Batch(batchSize))
                                    {
                                        await this.InsertRecordsToDb(batch.ToList());
                                    }

                                    await UpdateFileProcessedStatus(_serviceScopeFactory, fileToProcess);

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

                    _memoryCache.Set("VisionExtractorLock", false);

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
                _memoryCache.Set("VisionExtractorLock", false);
                _semaphore.Release();
            }
        }

        private async Task<List<EcommerceDbRecord>> GetRecordsFromVisionFile(string glFile)
        {
            this._logger.LogInformation($"Started Vision Record Extraction for {glFile.ToUpper()}");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<EcommerceDbRecord> glCmsRecs = new List<EcommerceDbRecord>();

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

        private void AddRecordsToList(IExcelDataReader reader, DateTime extractedDate, List<EcommerceDbRecord> glCmsRecs, string glFile)
        {
            var glRec = new EcommerceDbRecord
            {
                BankingDate = extractedDate,
                TransDetails = reader.GetValue(1)?.ToString(),
                TransID = reader.GetValue(2)?.ToString(),
                ReferenceNumber = reader.GetValue(3)?.ToString(),
                GLTransCode = reader.GetValue(4)?.ToString(),
                PaymentID = reader.GetValue(5)?.ToString(),
                CardNumber = reader.GetValue(6)?.ToString(),
                FinPostedAmount = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(7)?.ToString()) ? "0" : reader.GetValue(7)?.ToString()),
                CreditAmount = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(8)?.ToString()) ? "0" : reader.GetValue(8)?.ToString()),
                DebitAmount = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(9)?.ToString()) ? "0" : reader.GetValue(9)?.ToString()),
                EntryType = reader.GetValue(10)?.ToString(),
                EntryDirection = reader.GetValue(11)?.ToString(),
                EntryAmount = reader.GetValue(12)?.ToString(),
                TransCurr = Convert.ToInt32(string.IsNullOrEmpty(reader.GetValue(13)?.ToString()) ? "0" : reader.GetValue(13)?.ToString()),
                CrNumber = reader.GetValue(14)?.ToString(),
                DrNumber = reader.GetValue(15)?.ToString(),
                AuthorizationCode = reader.GetValue(16)?.ToString(),
                MerchantID = reader.GetValue(17)?.ToString(),
                FileName = glFile,
                DateExtracted = DateTime.Now
            };

            glCmsRecs.Add(glRec);
        }

        private async Task InsertRecordsToDb(List<EcommerceDbRecord> records)
        {
            _logger.LogInformation($"Started adding {records.Count} vision Ecommerce records to DbContext");

            Stopwatch sw = Stopwatch.StartNew();

            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                using (ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    await dbContext.EcommerceDbRecords.AddRangeAsync(records);

                    await dbContext.SaveChangesAsync();
                }

                this._logger.LogInformation($"Added {records.Count} vision Ecommerce records to DbContext in {sw.ElapsedMilliseconds} milliseconds");
            }
        }
    }
}
