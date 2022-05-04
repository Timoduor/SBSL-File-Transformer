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
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher;

namespace SbslFileTransformer.Converters.Kenya
{
    public class VisionRecordMatcher
    {
        private readonly ApplicationDbContext _dbContext;

        public VisionRecordMatcher(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task MatchFiles(string finacleFile, string outputPath, VisionRecordType visionRecordType)
        {
            List<FinacleRec> finacleRecords = this.GetRecordsFromFinacleFile(finacleFile, visionRecordType);

            IEnumerable<string> finacleRefs = finacleRecords.Select(f => f.ReferenceNumber).Distinct();

            foreach (string finRef in finacleRefs)
            {
                IEnumerable<VisionRecordBase> unmatchedVisionRecords = this.GetUnmatchedVisionRecords(visionRecordType);
                List<VisionRecordBase> matchedRecords = new List<VisionRecordBase>();

                if (finRef.Length != 20)
                    continue;

                if (!this.IsDigitsOnly(finRef))
                {
                    continue;
                }

                double finacleSumCredits = finacleRecords.Where(f => f.ReferenceNumber == finRef && f.DebitCredit == "Credit").Sum(f => f.Amount);
                double finacleSumDebits = finacleRecords.Where(f => f.ReferenceNumber == finRef && f.DebitCredit == "Debit").Sum(f => f.Amount);

                double finacleDiff = finacleSumCredits - finacleSumDebits;

                List<VisionRecordBase> matchedRecs = unmatchedVisionRecords.Where(v => v.ReferenceNumber == finRef).ToList();

                double visionCredits = matchedRecs.Sum(v => v.CreditAmount);
                double visionDebits = matchedRecs.Sum(v => v.DebitAmount);

                double visionDiff = visionCredits - visionDebits;

                if (Math.Abs(Math.Round(finacleDiff, 2)) == Math.Abs(Math.Round(visionDiff, 2)) && matchedRecs.Count() > 0)
                {
                    matchedRecords.AddRange(matchedRecs);

                    this.CreateFileForReferenceNumber(matchedRecs, finRef, outputPath, visionRecordType);

                    matchedRecords.ForEach(v =>
                    {
                        v.Matched = true;
                        v.DateMatched = DateTime.Now;
                        v.MatchingFile = finacleFile;
                    });

                    switch (visionRecordType)
                    {
                        case VisionRecordType.Collections:
                            this._dbContext.VisionRecordCollections.UpdateRange(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordCollection>(matchedRecords));
                            break;
                        case VisionRecordType.CreditSettlement:
                            this._dbContext.VisionRecordCreditSettlements.UpdateRange(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordCreditSettlement>(matchedRecords));
                            break;
                        case VisionRecordType.Debtors:
                            this._dbContext.VisionRecordDebtors.UpdateRange(VisionCommonHelpers.ConvertParentToChild<VisionRecordBase, VisionRecordDebtors>(matchedRecords));
                            break;
                    }
                    
                    await this._dbContext.SaveChangesAsync();
                }
            }
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private void CreateFileForReferenceNumber(IEnumerable<VisionRecordBase> matchedRecs, string referenceNumber, string outputPath, VisionRecordType visionRecordType)
        {
            string outputFile = Path.Combine(outputPath, $"{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}_{visionRecordType}_{referenceNumber}.csv");

            if (File.Exists(outputFile))
            {
                throw new Exception($"Vision ref no. {referenceNumber} file {outputFile} already exists");
            }

            this.GenerateFileForSelectedRecords(matchedRecs, outputFile);
        }

        private IEnumerable<VisionRecordBase> GetUnmatchedVisionRecords(VisionRecordType visionRecordType)
        {
            IEnumerable<VisionRecordBase> visionRecords = new List<VisionRecordBase>();

            switch (visionRecordType)
            {
                case VisionRecordType.Collections:
                    visionRecords = this._dbContext.VisionRecordCollections.Where(v => v.Matched == false).Select(r => (VisionRecordBase)r);
                    break;
                case VisionRecordType.CreditSettlement:
                    visionRecords = this._dbContext.VisionRecordCreditSettlements.Where(v => v.Matched == false).Select(r => (VisionRecordBase)r);
                    break;
                case VisionRecordType.Debtors:
                    visionRecords = this._dbContext.VisionRecordDebtors.Where(v => v.Matched == false).Select(r => (VisionRecordBase)r);
                    break;
            }

            return visionRecords;
        }

        private void GenerateFileForSelectedRecords(IEnumerable<VisionRecordBase> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (VisionRecordBase row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }

        private List<FinacleRec> GetRecordsFromFinacleFile(string cmsFile, VisionRecordType visionRecordType)
        {
            List<FinacleRec> finacleRecs = new List<FinacleRec>();

            using (FileStream stream = File.Open(cmsFile, FileMode.Open, FileAccess.Read))
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
                        FinacleRec finacleRec = new FinacleRec();

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
