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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class VisionRecordExtractorJob : ConverterJobBase<VisionRecordExtractorJob>, IHostedService
    {
        public VisionRecordExtractorJob(ILogger<VisionRecordExtractorJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Vision Record Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await VisionRecordExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 120)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task VisionRecordExtractor()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Record Matcher Extractor job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                        .ToList();

                    Entity = dbContext.Configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx"))
                       .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx")));

                    foreach (var file in files)
                    {
                        if (file.ToLower().Contains("cards") && file.ToLower().Contains("credit_card")
                            && file.ToLower().Contains("collections_cms") && file.ToLower().Contains("imke"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());



                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    var records = GetRecordsFromVisionFile(file);

                                    await InsertRecordsToDb(records, dbContext);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Extracting records from Vision files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(VisionRecordExtractorJob);

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

        private List<VisionRecord> GetRecordsFromVisionFile(string glFile)
        {
            var glCmsRecs = new List<VisionRecord>();

            using (var stream = File.Open(glFile, FileMode.Open, FileAccess.Read))
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
                        if (count <= 10)
                        {
                            count++;
                            continue;
                        }

                        VisionRecord glRec = new VisionRecord
                        {
                            BankingDate = Convert.ToDateTime(reader.GetString(0)),
                            TransDetails = reader.GetString(1),
                            TransID = reader.GetString(2),
                            ReferenceNumber = reader.GetString(3),
                            GLTransCode = reader.GetString(4),
                            CardNumber = reader.GetString(5),
                            CreditAmount = reader.GetDouble(6),
                            DebitAmount = reader.GetDouble(7),
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

        private async Task InsertRecordsToDb(IEnumerable<VisionRecord> records, ApplicationDbContext dbContext)
        {
            if (dbContext.VisionRecords.Any(v => v.FileName == records.First().FileName))
                return;

            dbContext.VisionRecords.AddRange(records);

            await dbContext.SaveChangesAsync();
        }
    }
}
