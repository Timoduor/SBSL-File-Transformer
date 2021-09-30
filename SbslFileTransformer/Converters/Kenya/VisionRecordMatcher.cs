using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SbslFileTransformer.Converters.Kenya
{
    public class VisionRecordMatcher
    {
        public void MatchFiles(string finacleFile, string outputPath)
        {
            ////List<VisionRecord> glRecs = GetVisionRecordsFromDb(glFile);
            //List<FinacleRec> cmsRecs = GetRecordsFromFinacleFile(finacleFile);

            ////foreach (var glRec in glRecs)
            //{
            //    //IEnumerable<FinacleRec> selectedRecs = cmsRecs.Where(c => c.RefNum == glRec.ReferenceNumber);

            //    double sumOfCmsCredit = selectedRecs.Select(c => c.Credit).Sum();
            //    double sumOfCmsDebit = selectedRecs.Select(c => c.Debit).Sum();

            //    //if (glRec.DebitAmount == sumOfCmsDebit && glRec.CreditAmount == sumOfCmsDebit)
            //    {
            //        var outputFilePath = Path.Combine(outputPath, Path.ChangeExtension(Path.GetFileName(glFile), ".csv"));

            //        GenerateFileForSelectedRecords(selectedRecs, outputFilePath);
            //    }
            //}
        }

        private void GenerateFileForSelectedRecords(IEnumerable<FinacleRec> rows, string outputFile)
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

                        finacleRec.TransDate = reader.GetDateTime(0);
                        finacleRec.TransTime = reader.GetString(1);
                        finacleRec.ValueDate = reader.GetDateTime(2);
                        finacleRec.TransID = reader.GetString(3);
                        finacleRec.TranParticular = reader.GetString(4);
                        finacleRec.TranRemarks = reader.GetString(5);
                        finacleRec.RefNum = reader.GetString(6);
                        finacleRec.Stan = reader.GetString(7);
                        finacleRec.TermID = reader.GetString(8);
                        finacleRec.DebitCredit = reader.GetString(9);
                        finacleRec.Credit = reader.GetDouble(10);
                        finacleRec.Debit = reader.GetDouble(11);
                        finacleRec.Outstanding = reader.GetDouble(12);
                        finacleRec.CustomerAccount = reader.GetString(13);

                        finacleRecs.Add(finacleRec);
                    }
                }
            }

            return finacleRecs;
        }



        public class FinacleRec
        {
            public DateTime TransDate { get; set; }
            public string TransTime { get; set; }
            public DateTime ValueDate { get; set; }
            public string TransID { get; set; }
            public string TranParticular { get; set; }
            public string TranRemarks { get; set; }
            public string RefNum { get; set; }
            public string Stan { get; set; }
            public string TermID { get; set; }
            public string DebitCredit { get; set; }
            public double Credit { get; set; }
            public double Debit { get; set; }
            public double Outstanding { get; set; }
            public string CustomerAccount { get; set; }
        }
    }
}
