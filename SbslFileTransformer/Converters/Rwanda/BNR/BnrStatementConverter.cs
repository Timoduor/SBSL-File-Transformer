using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;

using ExcelDataReader;

using SbslFileTransformer.Converters.Rwanda.BNR.Models;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.Rwanda.BNR
{
    public class BnrStatementConverter
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _entity;

        public BnrStatementConverter(string Entity, ApplicationDbContext dbContext)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _entity = Entity;
            _dbContext = dbContext;
        }

        public void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            var list = new List<CountHeader>();
            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var code = "Codes";
                    var status = "Status 2";
                    var DR_CR = "DR_CR";
                    var Type_id = "Type_id";
                    var debitOrCreditRowTitle = "";
                    var countHeader = 0;

                    while (reader.Read())
                    {
                        var row = new CountHeader();

                        var codeRowColumn = reader.GetValue(2)?.ToString();

                        if (codeRowColumn != null && codeRowColumn.StartsWith("Code"))
                        {
                            code = reader.GetValue(2)?.ToString();
                        }

                        //LOGIC FOR DEBIT/CREDIT COL FROM TITLE

                        var rec = reader.GetValue(3)?.ToString();

                        if (!string.IsNullOrEmpty(rec))
                        {
                            if (rec.StartsWith("Debit transactions"))
                            {
                                debitOrCreditRowTitle = "Debit";
                            }
                            else if (rec.StartsWith("Credit transactions"))
                            {
                                debitOrCreditRowTitle = "Credit";
                            }
                        }

                        //row with this col empty should never be empty in the final table
                        if (string.IsNullOrEmpty(reader.GetValue(4)?.ToString()))
                        {
                            continue;
                        }

                        //set the value for col15 in the final table
                        GetCol15TypeIdValue(code, row, reader);

                        //LOGIC FOR CHILD NODE

                        //The value at index 0 is null for the child row hence the check
                        if (reader.FieldCount >= 20 && ((reader.GetValue(19)?.ToString()?.Contains("Active") ?? false) || (reader.GetValue(19)?.ToString()?.Contains("Rejected") ?? false)))//merged columns are counted separately
                        {
                            //logic to read child columns

                            //Reference
                            row.Reference = reader.GetValue(4)?.ToString().Replace("\n", "");

                            //Codes colunm
                            row.Code = list.Last().Code;

                            //Value Date
                            row.Value_date = list.Last().Value_date;

                            //Type
                            row.Type = list.Last().Type;

                            //Debit account
                            row.Debit_account = list.Last().Debit_account;

                            //Odering customer
                            row.Ordering_customer = reader.GetValue(7) + reader.GetValue(10)?.ToString();

                            //Credit account
                            row.Credit_account = list.Last().Credit_account;

                            //Beneficiary
                            row.Beneficiary = reader.GetValue(13) + reader.GetValue(14)?.ToString();

                            //Remittance infos
                            row.Remittance_info = list.Last().Remittance_info;

                            //Amount
                            row.Amount = reader.GetValue(18)?.ToString();

                            //Input time
                            row.Input_time = list.Last().Input_time;

                            //Status
                            row.Status = list.Last().Status;

                            //Modification time
                            row.Modification_time = list.Last().Modification_time;

                            //(Active) Status of subdirectory 
                            var strLen = reader.GetValue(19)?.ToString().Length ?? 0;
                            var lengthToUse = strLen < 49 ? strLen : 49;
                            row.Status2 = reader.GetValue(19)?.ToString().Substring(0, lengthToUse);

                            //DR_CR
                            row.DR_CR = debitOrCreditRowTitle;

                            //method for the bulk colunm 
                            if (string.IsNullOrEmpty(list.Last().Status2?.ToLower()))
                            {
                                list.Last().Status2 = "Bulk";

                                if (list.Last().Status2?.Contains("pacs.008. 001.08") ?? false)
                                {
                                    list.Last().Type_id = "MT102";
                                }
                            }

                            list.Add(row);
                        }
                        else
                        {
                            //LOGIC FOR PARENT

                            //Reference
                            row.Reference = reader.GetValue(0)?.ToString().Replace("\n", "");

                            //Codes colunm 
                            row.Code = code;

                            //Value Date
                            row.Value_date = reader.GetValue(4)?.ToString();

                            //Type
                            row.Type = reader.GetValue(5)?.ToString();

                            //Debit Account
                            row.Debit_account = reader.GetValue(6)?.ToString().Replace("\n", "");

                            //Odering Customer/Drawer
                            row.Ordering_customer = reader.GetValue(8)?.ToString();

                            //Credit Account
                            row.Credit_account = reader.GetValue(11)?.ToString().Replace("\n", "");

                            //Beneficiary
                            row.Beneficiary = reader.GetValue(12)?.ToString();

                            //Remittance infos
                            row.Remittance_info = reader.GetValue(13)?.ToString();

                            //Amount
                            row.Amount = reader.GetValue(14)?.ToString();

                            //Input Time
                            row.Input_time = reader.GetValue(15)?.ToString();

                            //Status
                            var strLen = reader.GetValue(17)?.ToString().Length ?? 0;
                            var lengthToUse = strLen < 49 ? strLen : 49;
                            row.Status = reader.GetValue(17)?.ToString().Substring(0, lengthToUse);

                            //Modification Time
                            row.Modification_time = reader.GetValue(18)?.ToString();

                            //DR_CR
                            row.DR_CR = debitOrCreditRowTitle;

                            if (countHeader == 0)
                            {
                                row.Status2 = status;
                                row.DR_CR = DR_CR;
                                row.Type_id = Type_id;

                                countHeader++;
                            }

                            list.Add(row);
                        }
                    }
                }
            }

            var outputFolder = Path.GetDirectoryName(inputFile);

            outputFolder = Path.Combine(Directory.GetParent(outputFolder).FullName, "Conv");

            if (string.IsNullOrEmpty(outputFile))
            {
                _ = Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToUse = fileName.Substring(Math.Max(0, fileName.Length - 12)).Replace(" ", "");

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_{fileNameToUse}_STAMT.csv");
            }

            WriteToFile(list, outputFile);

            GetClosingBalanceMutliCurrNAdjustment(inputFile, rootFolder, outputFolder);
        }

        private void GetCol15TypeIdValue(string code, CountHeader row, IExcelDataReader reader)
        {
            if (code.Equals("Code - 032"))
            {
                row.Type_id = "MT104";
            }
            else if (code.Equals("Code - 035"))
            {
                row.Type_id = "MT104";
            }

            else if (code.Equals("Code - 012"))
            {
                row.Type_id = "MT971";
            }
            else if (code.Equals("Code - 011"))
            {
                row.Type_id = "MT971";
            }
            else if (code.Equals("Code - 010"))
            {
                row.Type_id = "MT971";
            }
            else if (!string.IsNullOrEmpty(reader.GetValue(5)?.ToString()) &&
                     !code.Equals("Code - 010") &&
                     !code.Equals("Code - 011") &&
                     !code.Equals("Code - 012") &&
                     (reader.GetValue(5).ToString()?.Equals("pacs.009. 001.08") ?? false))
            {
                row.Type_id = "MT202";
            }
            else if (reader.FieldCount >= 20)
            {
                if (!string.IsNullOrEmpty(reader.GetValue(19)?.ToString()) &&
                    !code.Equals("Code - 032") &&
                    ((reader.GetValue(19)?.ToString()?.Contains("Active") ?? false) ||
                     (reader.GetValue(19)?.ToString()?.Contains("Rejected") ?? false)))
                {
                    row.Type_id = "MT102";
                }
            }
            else if (!string.IsNullOrEmpty(row.Type) && row.Type.Contains("pacs.008. 001.08"))
            {
                row.Type_id = "MT102";
            }
            else
            {
                row.Type_id = "MT103";
            }
        }

        private void GetClosingBalanceMutliCurrNAdjustment(string inputFile, string rootFolder, string outputFolder)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    double closing = 0;
                    double opening = 0;
                    var row = new ExcelCols();
                    while (reader.Read())
                    {
                        if (reader.GetValue(15)?.ToString()?.Contains("Closing Balance") ?? false)
                        {
                            var val = reader.GetValue(15)?.ToString()?.Split(' ')[3];

                            closing = Convert.ToDouble(val);

                            if (closing != 0)
                            {
                                row.Col3 = closing.ToString();
                            }
                        }

                        if (reader.GetValue(9)?.ToString()?.Contains("Opening Balance") ?? false)
                        {
                            var val1 = reader.GetValue(9)?.ToString()?.Split(':')[1];

                            opening = Convert.ToDouble(val1);
                            if (opening != 0)
                            {
                                row.Col4 = opening.ToString();
                            }
                        }

                        if (reader.GetValue(0)?.ToString()?.Contains("Account:") ?? false)
                        {
                            row.Col1 = reader.GetValue(0)?.ToString()?
                                .Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        }

                        if (reader.GetValue(7)?.ToString()?.StartsWith("Date From") ?? false)
                        {
                            var data = reader.GetValue(7)?.ToString();
                            var lines = data?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var line in lines)
                            {
                                if (line.StartsWith("Currency:"))
                                {
                                    var currency = line.Split(':')[1];

                                    row.Col2 = currency;
                                }
                                else if (line.StartsWith("Date From"))
                                {
                                    var date = DateTime.ParseExact(line.Split(' ')[2], "dd-MM-yyyy",
                                        CultureInfo.InvariantCulture);
                                    row.Col0 = date.ToString("dd-MM-yyyy");
                                }
                            }
                        }
                    }

                    //Difference between closing balance and opening balance
                    row.Col5 = (Convert.ToDouble(row.Col3) - Convert.ToDouble(row.Col4)).ToString();
                    list.Add(row);
                }
            }

            GenerateAdjustment(list, inputFile, outputFolder);

            //input = acc / amt / date
            GenerateMultiCurr(list, inputFile, rootFolder);
        }

        private void GenerateAdjustment(List<ExcelCols> list, string inputFile, string outputFolder)
        {
            var countHeader = new CountHeader
            {
                Value_date = list.First().Col0.Replace("/", "-"),

                Amount = list.First().Col5,

                Remittance_info = "Adjust. clearing BNR for " + list.First().Col0
            };

            //RWF
            if (list.First().Col5.Contains("-") && list.First().Col1.Contains("1240000"))
            {
                countHeader.Debit_account = "1240000";
                countHeader.DR_CR = "Debit";
                //EUR
            }
            else if (list.First().Col5.Contains("-") && list.First().Col1.Contains("1000026561"))
            {
                countHeader.Debit_account = "1000026561";
                countHeader.DR_CR = "Debit";
                //USD
            }
            else if (list.First().Col5.Contains("-") && list.First().Col1.Contains("3208000"))
            {
                countHeader.Debit_account = "3208000";
                countHeader.DR_CR = "Debit";
            }
            //RWF
            else if (list.First().Col5.Contains("") && list.First().Col1.Contains("1240000"))
            {
                countHeader.Credit_account = "1240000";
                countHeader.DR_CR = "Credit";
            }
            //EUR
            else if (list.First().Col5.Contains("") && list.First().Col1.Contains("1000026561"))
            {
                countHeader.Credit_account = "1000026561";
                countHeader.DR_CR = "Credit";
            }
            //USD
            else if (list.First().Col5.Contains("") && list.First().Col1.Contains("3208000"))
            {
                countHeader.Credit_account = "3208000";
                countHeader.DR_CR = "Credit";
            }

            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            countHeader.Amount = countHeader.Amount.TrimStart('-');

            WriteToFile(countHeader,
                Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_{fileNameToAppend}_ADJSMT.csv"));
        }

        private void GenerateMultiCurr(List<ExcelCols> list, string inputFile, string outputFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(outputFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_BNR_{_entity}.txt");

            var toAppend = new StringBuilder();

            var account = list.First().Col1;


            if (!DateTime.TryParseExact(list.First().Col0, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            {
                throw new Exception("Failed to convert provided datetime");
            }

            var amount = list.First().Col3; //vs col5 diff
            var currency = list.First().Col2;

            _ = toAppend.Append(
                $"{_entity}\t{GetGLAccountNumber(account, _dbContext)}\tNostros\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            var text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllText(outputFile, text);
            }
        }

        private string GetGLAccountNumber(string accNo, ApplicationDbContext dbContext)
        {
            var account = dbContext.Accounts.FirstOrDefault(a =>
                a.Account.ToLower() == accNo.ToLower() || a.Account.ToLower().Contains(accNo.ToLower()))?.Number;

            if (!string.IsNullOrEmpty(account))
            {
                return account;
            }

            return accNo;
        }

        private void WriteToFile(List<CountHeader> rows, string outputFile)
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

        private void WriteToFile(CountHeader rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<CountHeader>();
                    csv.NextRecord();
                    csv.WriteRecord(rows);
                }
            }
        }
    }
}
