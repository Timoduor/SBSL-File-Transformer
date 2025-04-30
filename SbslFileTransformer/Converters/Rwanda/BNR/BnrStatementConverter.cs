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
            var list = new List<ExcelCols>();
            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var code = "Codes";
                    var status = "Status 2";
                    var DR_CR = "DR_CR";
                    var Type_id = "Type_id";
                    var Title = "";
                    var countHeader = 0;
                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        var val1 = reader.GetValue(0)?.ToString();

                        var val = reader.GetValue(2)?.ToString();

                        if (val != null && reader.GetValue(2).ToString().StartsWith("Code"))
                        {
                            code = reader.GetValue(2)?.ToString();
                        }

                        //Logic for title
                        if (reader.GetValue(3) != null)
                        {
                            var rec = reader.GetValue(3).ToString();
                            if (rec.StartsWith("Debit transactions"))
                            {
                                Title = "Debit";
                            }
                            else if (rec.StartsWith("Credit transactions"))
                            {
                                Title = "Credit";
                            }
                        }

                        var value = reader.GetValue(4)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }


                        if (code.Equals("Code - 032"))
                        {
                            row.Col15 = "MT104";
                        }
                        else if (code.Equals("Code - 035"))
                        {
                            row.Col15 = "MT104";
                        }

                        //if (row.Col3 != null && code.Equals("Code - 035") 
                        //    && row.Col3.Contains("pacs.003. 001.08"))
                        //{
                        //    row.Col15 = "MT104";
                        //}

                        else if (code.Equals("Code - 012"))
                        {
                            row.Col15 = "MT971";
                        }
                        else if (code.Equals("Code - 011"))
                        {
                            row.Col15 = "MT971";
                        }
                        else if (code.Equals("Code - 010"))
                        {
                            row.Col15 = "MT971";
                        }

                        //else if (code.Equals("Code - 010") && val1.StartsWith("MACA"))
                        //{
                        //    row.Col15 = "MT971";
                        //}

                        //else if (code.Equals("Code - 010") && !val1.StartsWith("MACA"))
                        //{
                        //    row.Col15 = "MT202";
                        //}

                        else if (reader.GetValue(5) != null &&
                          !code.Equals("Code - 010") &&
                          !code.Equals("Code - 011") &&
                          !code.Equals("Code - 012") &&
                          reader.GetValue(5).ToString().Equals("pacs.009. 001.08"))
                        {
                            row.Col15 = "MT202";
                        }

                        else if (reader.GetValue(19) != null &&
                          !code.Equals("Code - 032") &&
                          reader.GetValue(19)?.ToString() == "Active" ||
                          reader.GetValue(19)?.ToString() == "Rejected")
                        {
                            row.Col15 = "MT102";

                        }

                        else if (row.Col3 != null && row.Col13 != null &&
                            row.Col3.Contains("pacs.008. 001.08") &&
                            row.Col13.Contains("Bulk"))
                        {
                            row.Col15 = "MT102";
                        }
                        else
                        {
                            row.Col15 = "MT103";
                        }


                        //logic for child node
                        //The value at index 0 is null for the child row hence the check
                        if (reader.GetValue(19)?.ToString() is "Active" or "Rejected")
                        {
                            //logic to read child columns

                            //Reference
                            row.Col0 = reader.GetValue(4)?.ToString().Replace("\n", "");

                            //Codes colunm
                            row.Col1 = list.Last().Col1;
                            //Value Date
                            row.Col2 = list.Last().Col2;

                            row.Col3 = list.Last().Col3;
                            //Debit account
                            row.Col4 = list.Last().Col4;
                            //Odering customer
                            row.Col5 = reader.GetValue(7)?.ToString() + reader.GetValue(10)?.ToString();
                            //Credit account
                            row.Col6 = list.Last().Col6;

                            row.Col7 = reader.GetValue(13)?.ToString() + reader.GetValue(14)?.ToString();

                            row.Col8 = list.Last().Col8;
                            //Amount
                            row.Col9 = reader.GetValue(18)?.ToString(); ;
                            //Input time
                            row.Col10 = list.Last().Col10;
                            //Status
                            row.Col11 = list.Last().Col11;
                            //Modification time
                            row.Col12 = list.Last().Col12;
                            //(Active) Status of subdirectory 
                            row.Col13 = reader.GetValue(19)?.ToString();
                            //DR_CR
                            row.Col14 = Title;
                            //method for the bulk colunm 
                            if (string.IsNullOrEmpty(list.Last().Col13?.ToLower()))
                            {
                                list.Last().Col13 = "Bulk";
                                if (list.Last().Col3.Contains("pacs.008. 001.08"))
                                {
                                    list.Last().Col15 = "MT102";
                                }
                            }
                            list.Add(row);
                        }

                        else
                        {
                            //logic for parent


                            //Reference
                            row.Col0 = reader.GetValue(0)?.ToString().Replace("\n", "");

                            //Codes colunm 
                            row.Col1 = code;

                            //Value Date
                            row.Col2 = reader.GetValue(4)?.ToString();

                            //Type
                            row.Col3 = reader.GetValue(5)?.ToString();

                            //Debit Account
                            row.Col4 = reader.GetValue(6)?.ToString().Replace("\n", "");

                            //Odering Customer/Drawer
                            row.Col5 = reader.GetValue(8)?.ToString();

                            //Credit Account
                            row.Col6 = reader.GetValue(11)?.ToString().Replace("\n", "");

                            //Beneficiary
                            row.Col7 = reader.GetValue(12)?.ToString();

                            //Remittance infos
                            row.Col8 = reader.GetValue(13)?.ToString();

                            //Amount
                            row.Col9 = reader.GetValue(14)?.ToString();

                            //Input Time
                            row.Col10 = reader.GetValue(15)?.ToString();

                            //Status
                            var strLen = reader.GetValue(17)?.ToString().Length ?? 0;
                            var lengthToUse = strLen < 48 ? strLen : 49;
                            row.Col11 = reader.GetValue(17)?.ToString().Substring(0, lengthToUse);

                            //Modification Time
                            row.Col12 = reader.GetValue(18)?.ToString();
                            //DR_CR
                            row.Col14 = Title;

                            if (countHeader == 0)
                            {
                                row.Col13 = status;
                                row.Col14 = DR_CR;
                                row.Col15 = Type_id;

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
                        if (reader.GetValue(15)?.ToString().Contains("Closing Balance") ?? false)
                        {
                            var val = reader.GetValue(15).ToString().Split(' ')[3];

                            closing = Convert.ToDouble(val);

                            if (closing != 0)
                            {
                                row.Col3 = closing.ToString();
                            }
                        }

                        if (reader.GetValue(9)?.ToString().Contains("Opening Balance") ?? false)
                        {
                            var val1 = reader.GetValue(9).ToString().Split(':')[1];

                            opening = Convert.ToDouble(val1);
                            if (opening != 0)
                            {
                                row.Col4 = opening.ToString();
                            }
                        }

                        if (reader.GetValue(0)?.ToString().Contains("Account:") ?? false)
                        {
                            row.Col1 = reader.GetValue(0)?.ToString()
                                .Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        }

                        if (reader.GetValue(7)?.ToString().StartsWith("Date From") ?? false)
                        {
                            var data = reader.GetValue(7)?.ToString();
                            var lines = data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

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

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
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
