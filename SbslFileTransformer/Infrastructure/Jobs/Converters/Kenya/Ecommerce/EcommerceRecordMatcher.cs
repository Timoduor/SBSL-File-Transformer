using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CsvHelper;

using ExcelDataReader;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MySqlConnector;

using SbslFileTransformer.Converters.Kenya;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.Ecommerce.Models;
using SbslFileTransformer.Infrastructure.Messaging;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.Ecommerce
{
    public class EcommerceRecordMatcher
    {
        private readonly ILogger<EcommerceMatchingJob> _logger;
        private readonly EmailSender _emailSender;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EcommerceRecordMatcher(IServiceScopeFactory serviceScopeFactory, ILogger<EcommerceMatchingJob> logger, EmailSender emailSender)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
            _emailSender = emailSender;
        }

        public async Task MatchFiles(string finacleFile, string outputPath)
        {
            var stopwatch = Stopwatch.StartNew();

            var finacleRecords = await Task.Run(() => GetRecordsFromFinacleFile(finacleFile));

            _logger.LogWarning($"{finacleFile.ToUpper()} took {stopwatch.ElapsedMilliseconds / 1000} seconds to extract {finacleRecords.Count} records");

            if (finacleRecords.Count == 0)
            {
                _logger.LogWarning($"No records found in {finacleFile.ToUpper()}");

                await _emailSender.SendMessage(null, "No records found in", $"No records found in {finacleFile.ToUpper()}");

                return;
            }

            var finacleRefs = finacleRecords.Select(f => new Tuple<string, string>(f.ReferenceNumber, f.AccountNumber)).Distinct();

            stopwatch.Restart();

            List<EcommerceDbRecord> matchedRecords = new List<EcommerceDbRecord>();

            foreach (Tuple<string, string> finRef in finacleRefs)
            {
                if (finRef.Item1.Length != 20)
                    continue;

                if (!this.IsDigitsOnly(finRef.Item1))
                {
                    continue;
                }

                double finacleSumCredits = finacleRecords.Where(f => f.ReferenceNumber == finRef.Item1 && f.AccountNumber == finRef.Item2 && f.DebitCredit == "Credit").Sum(f => f.Amount);
                double finacleSumDebits = finacleRecords.Where(f => f.ReferenceNumber == finRef.Item1 && f.AccountNumber == finRef.Item2 && f.DebitCredit == "Debit").Sum(f => f.Amount);

                double finacleDiff = finacleSumCredits - finacleSumDebits;

                List<EcommerceDbRecord> matchedRecs = await GetUnmatchedVisionRecords(finRef.Item1, finRef.Item2);

                double visionCredits = matchedRecs.Sum(v => v.CreditAmount);
                double visionDebits = matchedRecs.Sum(v => v.DebitAmount);

                double visionDiff = visionCredits - visionDebits;

                if (Math.Abs(Math.Round(finacleDiff, 2)) == Math.Abs(Math.Round(visionDiff, 2)) && matchedRecs.Count() > 0)
                {
                    string finacleAccount = finacleRecords.FirstOrDefault(f => f.ReferenceNumber == finRef.Item1 && f.AccountNumber == finRef.Item2)?.AccountNumber;

                    matchedRecs.ForEach(v =>
                    {
                        v.Matched = true;
                        v.DateMatched = DateTime.Now;
                        v.MatchingFile = Path.GetFileName(finacleFile);
                        v.FinacleAccount = finacleAccount;
                    });

                    await CreateFileForReferenceNumber(matchedRecs, finRef.Item1, finRef.Item2, outputPath);

                    matchedRecords.AddRange(matchedRecs);

                    _logger.LogInformation($"Found {matchedRecs.Count} matched records for ref No {finRef.Item1} acc No {finRef.Item2}");
                }
            }

            await this.UpdateVisionRecords(matchedRecords);

            matchedRecords.Clear();

            _logger.LogWarning($"It took {stopwatch.ElapsedMilliseconds / 1000} seconds to match records for Finacle file: {finacleFile}");
        }

        private async Task UpdateVisionRecords(List<EcommerceDbRecord> matchedRecords)
        {
            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                IConfiguration configuration = scope.ServiceProvider.GetService<IConfiguration>();

                string connectionString = configuration.GetConnectionString("DefaultConnection");

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var matched in matchedRecords.Batch(50))
                    {
                        var records = matched.ToArray();

                        using (var command = connection.CreateCommand())
                        {
                            for (int i = 0; i < records.Count(); i++)
                            {
                                command.CommandText += $"UPDATE ecommercedbrecords SET Matched = 1, DateMatched = @DateMatched{i}, MatchingFile = @MatchingFile{i}, FinacleAccount = @FinacleAccount{i} WHERE Id = @Id{i} LIMIT 1;";

                                command.Parameters.AddWithValue($"@DateMatched{i}", records[i].DateMatched);
                                command.Parameters.AddWithValue($"@MatchingFile{i}", records[i].MatchingFile);
                                command.Parameters.AddWithValue($"@FinacleAccount{i}", records[i].FinacleAccount);
                                command.Parameters.AddWithValue($"@Id{i}", records[i].Id);
                            }
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (var c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private async Task CreateFileForReferenceNumber(IEnumerable<EcommerceDbRecord> matchedRecs, string referenceNumber, string accountNumber, string outputPath)
        {
            var outputFile = Path.Combine(outputPath, $"{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}_{referenceNumber}_{accountNumber}.csv");

            if (File.Exists(outputFile))
            {
                throw new Exception($"Vision Ref No. {referenceNumber} and A/C No. {accountNumber} file {outputFile} already exists");
            }

            await GenerateFileForSelectedRecords(matchedRecs, outputFile);
        }

        private async Task<List<EcommerceDbRecord>> GetUnmatchedVisionRecords(string refNumber, string accNumber)
        {
            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var visionRecords = await dbContext.EcommerceDbRecords.Where(v => !v.Matched && v.ReferenceNumber == refNumber && (v.CrNumber == accNumber || v.DrNumber == accNumber)).ToListAsync();

                return visionRecords;
            }
        }

        private async Task GenerateFileForSelectedRecords(IEnumerable<EcommerceDbRecord> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(rows);
                }
                //await writer.FlushAsync();
            }
        }

        private List<FinacleRec> GetRecordsFromFinacleFile(string cmsFile)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var finacleRecs = new List<FinacleRec>();

            var count = 0;

            using (var stream = File.Open(cmsFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(cmsFile).Contains("csv", StringComparison.OrdinalIgnoreCase))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    while (reader.Read())
                    {
                        var finacleRec = new FinacleRec();

                        if (reader.FieldCount < 18)
                        {
                            continue;
                        }

                        finacleRec.AccountNumber = reader.GetString(0);
                        finacleRec.Currency = reader.GetString(1);
                        finacleRec.ReferenceNumber = reader.GetString(2);
                        finacleRec.CardNumber = reader.GetString(3);
                        finacleRec.TransDate = reader.GetString(4);
                        if (DateTime.TryParseExact(reader.GetString(5), "dd-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var valueDate))
                        {
                            finacleRec.ValueDate = valueDate;
                        }
                        finacleRec.TransactionTime = reader.GetString(6);
                        finacleRec.Ref1 = reader.GetString(7);
                        finacleRec.Ref2 = reader.GetString(8);
                        finacleRec.Ref3 = reader.GetString(9);
                        finacleRec.Ref4 = reader.GetString(10);
                        finacleRec.DebitCredit = reader.GetString(11);
                        if (Double.TryParse(reader.GetString(12), out double amount))
                        {
                            finacleRec.Amount = amount;
                        }
                        finacleRec.TransactionParticular = reader.GetString(13);
                        finacleRec.TransactionID = reader.GetString(14);
                        if (DateTime.TryParseExact(reader.GetString(15), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var transDate))
                        {
                            finacleRec.TransactionDate = transDate;
                        }
                        finacleRec.Time = reader.GetString(16);
                        finacleRec.Ref5 = reader.GetString(17);
                        finacleRec.Branch = reader.GetString(18);

                        finacleRecs.Add(finacleRec);

                        count++;
                    }
                }
            }

            return finacleRecs;
        }
    }
}
