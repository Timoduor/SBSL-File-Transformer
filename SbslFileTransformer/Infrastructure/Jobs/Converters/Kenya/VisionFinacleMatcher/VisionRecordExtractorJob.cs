using ExcelDataReader;
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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class VisionRecordExtractorJob : ConverterJobBase<VisionRecordExtractorJob>, IHostedService
    {
        int batchSize = 5000;

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
                                    Stopwatch recordReader = new Stopwatch();
                                    recordReader.Start();

                                    List<VisionRecordBase> records = await this.GetRecordsFromVisionFile(file);

                                    this._logger.LogInformation($"It took {recordReader.ElapsedMilliseconds / 1000} seconds to READ {records.Count} vision records from file");

                                    recordReader.Restart();

                                    List<Task> tasks = new List<Task>();

                                    foreach (IEnumerable<VisionRecordBase> batch in records.Batch(batchSize))
                                    {
                                        tasks.Add(this.InsertRecordsToDb(batch.ToList()));
                                    }

                                    await Task.WhenAll(tasks);

                                    this._logger.LogInformation($"It took {recordReader.ElapsedMilliseconds / 1000} seconds to SAVE the records to the database");
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<VisionRecordBase> glCmsRecs = new List<VisionRecordBase>();

            using (FileStream stream = File.Open(glFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(glFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = t => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = false
                    }
                });

                foreach (IEnumerable<DataRow> rows in dataSet.Tables[0].Rows.OfType<DataRow>().Batch(batchSize))
                {
                    AddRecordsToList(rows, glCmsRecs, glFile);
                }
                stream.Close();
            }

            return glCmsRecs;
        }

        private void AddRecordsToList(IEnumerable<DataRow> rows, List<VisionRecordBase> glCmsRecs, string glFile)
        {
            foreach (var row in rows)
            {
                if (!DateTime.TryParse(row[0]?.ToString(), out DateTime result))
                    continue;

                var glRec = new VisionRecordCollection
                {
                    BankingDate = result,
                    TransDetails = row[1]?.ToString(),
                    TransID = row[2]?.ToString(),
                    ReferenceNumber = row[3]?.ToString(),
                    GLTransCode = row[4]?.ToString(),
                    CardNumber = row[5]?.ToString(),
                    CreditAmount = Convert.ToDouble(row[6]?.ToString()),
                    DebitAmount = Convert.ToDouble(row[7]?.ToString()),
                    CustomerName = row[8]?.ToString(),
                    ContractNumber = row[9]?.ToString(),
                    AccountNumber = row[10]?.ToString(),
                    ChequeNo = row[11]?.ToString(),
                    AuthorizationCode = row[12]?.ToString(),
                    FileName = glFile,
                    DateExtracted = DateTime.Now
                };

                glCmsRecs.Add(glRec);
            }
        }

        private async Task InsertRecordsToDb(List<VisionRecordBase> records)
        {
            if (records == null || !records.Any())
                return;
            
            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                using (ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    dbContext.VisionRecordCollections.AddRange(
                        VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordCollection>(records));

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
