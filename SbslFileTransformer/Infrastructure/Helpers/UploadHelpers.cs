using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using SbslFileTransformer.Data;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class UploadHelpers
    {
        /// <summary>
        /// returns string with all the accounts that were not uploaded because they are already contained in the DB
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> ProcessedAccountsExcelUpload(string inputFile, ApplicationDbContext dbContext)
        {
            StringBuilder toReturn = new StringBuilder();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().EndsWith(".csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    while (reader.Read())
                    {
                        string entity = reader.GetValue(1)?.ToString();
                        string name = reader.GetValue(2)?.ToString();
                        string number = reader.GetValue(3)?.ToString();
                        string account = reader.GetValue(4)?.ToString();
                        string currency = reader.GetValue(5)?.ToString();

                        if (dbContext.Accounts.Any(x => x.Number == number))
                        {
                            toReturn.AppendLine($"{entity} {name} {number} {account} {currency} already exists! Updating it instead {Environment.NewLine}");

                            var record = dbContext.Accounts.FirstOrDefault(x => x.Number == number);

                            record.Account = account;
                            record.Currency = currency;
                            record.Entity = entity;
                            record.Name = name;

                            dbContext.Accounts.Update(record);
                        }
                        else
                        {
                            await dbContext.Accounts.AddAsync(new Models.AccountsLookup
                            {
                                Account = account,
                                Name = name,
                                Number = number,
                                Currency = currency,
                                Entity = entity
                            });
                        }
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            return toReturn.ToString();
        }

        /// <summary>
        /// returns string with all the escalations that were not uploaded because they are already contained in the DB
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> ProcessEscalationsExcelUpload(string inputFile, ApplicationDbContext dbContext)
        {
            StringBuilder toReturn = new StringBuilder();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().EndsWith(".csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    while (reader.Read())
                    {
                        string description = reader.GetValue(1)?.ToString();
                        string nameKeywords = reader.GetValue(2)?.ToString();
                        string columnKeywords = reader.GetValue(3)?.ToString();
                        string daysOverdue = reader.GetValue(4)?.ToString();
                        string recipientEmails = reader.GetValue(5)?.ToString();
                        bool isManagerReport = reader.GetValue(6)?.ToString().ToLower() == "true";

                        if (!int.TryParse(daysOverdue, out int result))
                            continue;

                        try
                        {
                            await dbContext.ReportConfigurations.AddAsync(new Models.ReportConfiguration
                            {
                                ReportDescription = description,
                                NameKeywords = nameKeywords,
                                ColumnKeywords = columnKeywords,
                                DaysOverdue = result,
                                RecipientEmails = recipientEmails,
                                IsManagerReport = isManagerReport,
                                IsEnabled = true
                            });

                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            toReturn.AppendLine($"Failed to create escalation with values \"{description}\" \"{nameKeywords}\" \"{columnKeywords}\" \"{daysOverdue}\" \"{recipientEmails}\"! {ex.Message} {Environment.NewLine}");
                        }
                    }
                }
            }

            return toReturn.ToString();
        }
    }
}
