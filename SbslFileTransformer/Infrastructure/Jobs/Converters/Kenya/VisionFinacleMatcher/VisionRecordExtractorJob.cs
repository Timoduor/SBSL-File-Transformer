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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class VisionRecordExtractorJob : ConverterJobBase<VisionRecordExtractorJob>, IHostedService
    {
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
                                    List<VisionRecordBase> records = this.GetRecordsFromVisionFile(file);

                                    await this.InsertRecordsToDb(records, dbContext, visionRecordType);
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
        
        private List<VisionRecordBase> GetRecordsFromVisionFile(string glFile)
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

                using (reader)
                {
                    int count = 0;

                    while (reader.Read())
                    {
                        if (count < 1)
                        {
                            count++;
                            continue;
                        }

                        if (!DateTime.TryParse(reader.GetString(0), out DateTime result))
                            return new List<VisionRecordBase>();

                        var glRec = new VisionRecordCollection
                        {
                            BankingDate = result,
                            TransDetails = reader.GetString(1),
                            TransID = reader.GetString(2),
                            ReferenceNumber = reader.GetString(3),
                            GLTransCode = reader.GetString(4),
                            CardNumber = reader.GetString(5),
                            CreditAmount = Convert.ToDouble(reader.GetString(6)),
                            DebitAmount = Convert.ToDouble(reader.GetString(7)),
                            CustomerName = reader.GetString(8),
                            ContractNumber = reader.GetString(9),
                            AccountNumber = reader.GetString(10),
                            FileName = glFile,
                            DateExtracted = DateTime.Now
                        };

                        glCmsRecs.Add(glRec);
                    }
                }
            }
            return glCmsRecs;
        }

        private async Task InsertRecordsToDb(List<VisionRecordBase> records, ApplicationDbContext dbContext, VisionRecordType visionRecordType)
        {
            if (records == null || !records.Any())
                return;

            var fileName = records.First().FileName.ToLower();

            if (
                dbContext.VisionRecordCollections.Any(v => v.FileName.ToLower() == fileName)
                || dbContext.VisionRecordCreditSettlements.Any(v => v.FileName.ToLower() == fileName)
                || dbContext.VisionRecordDebtors.Any(v => v.FileName.ToLower() == fileName)
               )
            {
                return;
            }

            switch (visionRecordType)
            {
                case VisionRecordType.Collections:
                    dbContext.VisionRecordCollections.AddRange(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordCollection>(records));
                    break;
                case VisionRecordType.CreditSettlement:
                    dbContext.VisionRecordCreditSettlements.AddRange(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordCreditSettlement>(records));
                    break;
                case VisionRecordType.Debtors:
                    dbContext.VisionRecordDebtors.AddRange(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordDebtors>(records));
                    break;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
