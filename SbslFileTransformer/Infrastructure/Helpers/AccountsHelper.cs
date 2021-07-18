using ExcelDataReader;
using SbslFileTransformer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class AccountsHelper
    {
        /// <summary>
        /// returns string with all the accounts that were not uploaded because they are already contained in the DB
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> ProcessedExcelUpload(string inputFile, ApplicationDbContext dbContext)
        {
            var toReturn = new StringBuilder();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
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
                        var entity = reader.GetValue(1)?.ToString();
                        var name = reader.GetValue(2)?.ToString();
                        var number = reader.GetValue(3)?.ToString();
                        var account = reader.GetValue(4)?.ToString();
                        var currency = reader.GetValue(5)?.ToString();

                        if (dbContext.Accounts.Any(x => x.Number == number))
                        {
                            toReturn.AppendLine($"{entity} {name} {number} {account} {currency} already exists! {Environment.NewLine}");
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

                            await dbContext.SaveChangesAsync();
                        }

                    }
                }
            }

            return toReturn.ToString();
        }
    }
}