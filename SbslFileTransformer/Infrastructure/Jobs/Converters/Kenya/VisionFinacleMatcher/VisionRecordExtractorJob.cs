using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
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
                TimeSpan.FromSeconds(new Random().Next(15, 60)), TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async Task VisionRecordExtractor()
        {

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

                        var glRec = new VisionRecord();

                        glRec.BankingDate = reader.GetDateTime(0);
                        glRec.TransDetails = reader.GetString(1);
                        glRec.TransID = reader.GetString(2);
                        glRec.ReferenceNumber = reader.GetString(3);
                        glRec.GLTransCode = reader.GetString(4);
                        glRec.CardNumber = reader.GetString(5);
                        glRec.CreditAmount = reader.GetDouble(6);
                        glRec.DebitAmount = reader.GetDouble(7);
                        glRec.CustomerName = reader.GetString(8);
                        glRec.ContractNumber = reader.GetString(9);
                        glRec.AccountNumber = reader.GetString(10);
                        glRec.FileName = glFile;
                        glRec.DateProcessed = DateTime.Now;

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
