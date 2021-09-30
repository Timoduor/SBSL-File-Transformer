using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters.Kenya
{
    public class VisionRecordMatcher
    {
        private ApplicationDbContext _dbContext;

        public VisionRecordMatcher(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MatchFiles(string finacleFile, string outputPath)
        {
            List<FinacleRec> finacleRecords = GetRecordsFromFinacleFile(finacleFile);

            IEnumerable<VisionRecord> unmatchedVisionRecords = GetUnmatchedVisionRecords();

            List<VisionRecord> matchedRecords = new List<VisionRecord>();

            foreach (var finRec in finacleRecords)
            {
                if (unmatchedVisionRecords.Any(u => u.ReferenceNumber == finRec.ReferenceNumber))
                {
                    IEnumerable<VisionRecord> matchedRecs = unmatchedVisionRecords.Where(u => u.ReferenceNumber == finRec.ReferenceNumber);

                    matchedRecords.AddRange(matchedRecs);

                    CreateFileForReferenceNumber(matchedRecs, finRec.ReferenceNumber, outputPath);
                }
            }

            matchedRecords.ForEach(v => v.Matched = true);

            _dbContext.VisionRecords.UpdateRange(matchedRecords);
            await _dbContext.SaveChangesAsync();
        }

        private void CreateFileForReferenceNumber(IEnumerable<VisionRecord> matchedRecs, string referenceNumber, string outputPath)
        {
            string outputFile = Path.Combine(outputPath, referenceNumber + ".csv");

            if (File.Exists(outputFile))
            {
                throw new Exception($"Vision ref no. {referenceNumber} file {outputFile} already exists");
            }

            GenerateFileForSelectedRecords(matchedRecs, outputFile);
        }

        private IEnumerable<VisionRecord> GetUnmatchedVisionRecords()
        {
            return _dbContext.VisionRecords.Where(v => v.Matched);
        }

        private void GenerateFileForSelectedRecords(IEnumerable<VisionRecord> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }

        private List<FinacleRec> GetRecordsFromFinacleFile(string cmsFile)
        {
            var finacleRecs = new List<FinacleRec>();

            using (var stream = File.Open(cmsFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(cmsFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    while (reader.Read())
                    {
                        var finacleRec = new FinacleRec();

                        finacleRec.AccountNumber = reader.GetString(0);
                        finacleRec.Currency = reader.GetString(1);
                        finacleRec.ReferenceNumber = reader.GetString(2);
                        finacleRec.CardNumber = reader.GetString(3);
                        finacleRec.TransDate = reader.GetString(4);
                        finacleRec.ValueDate = Convert.ToDateTime(reader.GetString(5));
                        finacleRec.TransactionTime = reader.GetString(6);
                        finacleRec.Ref1 = reader.GetString(7);
                        finacleRec.Ref2 = reader.GetString(8);
                        finacleRec.Ref3 = reader.GetString(9);
                        finacleRec.Ref4 = reader.GetString(10);
                        finacleRec.DebitCredit = reader.GetString(11);
                        finacleRec.Amount = Convert.ToDouble(reader.GetString(12));
                        finacleRec.TransactionParticular = reader.GetString(13);
                        finacleRec.TransactionID = reader.GetString(14);
                        if (DateTime.TryParseExact(reader.GetString(15), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime transDate))
                        {
                            finacleRec.TransactionDate = transDate;
                        }
                        finacleRec.Time = reader.GetString(16);
                        finacleRec.Ref5 = reader.GetString(17);
                        finacleRec.Branch = reader.GetString(18);

                        finacleRecs.Add(finacleRec);
                    }
                }
            }

            return finacleRecs;
        }



        public class FinacleRec
        {
            public string AccountNumber { get; set; }
            public string Currency { get; set; }
            public string ReferenceNumber { get; set; }
            public string CardNumber { get; set; }
            public string TransDate { get; set; }
            public DateTime ValueDate { get; set; }
            public string TransactionTime { get; set; }
            public string Ref1 { get; set; }
            public string Ref2 { get; set; }
            public string Ref3 { get; set; }
            public string Ref4 { get; set; }
            public string DebitCredit { get; set; }
            public double Amount { get; set; }
            public string TransactionParticular { get; set; }
            public string TransactionID { get; set; }
            public DateTime TransactionDate { get; set; }
            public string Time { get; set; }
            public string Ref5 { get; set; }
            public string Branch { get; set; }
        }
    }
}
