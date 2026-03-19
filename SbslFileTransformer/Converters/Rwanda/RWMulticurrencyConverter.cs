using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace RWMulticurrency_Converter
{
    public class RWMulticurrencyConverter
    {
        // Convert now RETURNS the USD total
        public double Convert(string balancesPath, string dailyRatesPath, string outputCsvPath)
        {
            Console.WriteLine("Reading balances file...");
            var balances = ReadExcel(balancesPath);

            Console.WriteLine("Reading daily conversion rates file...");
            var rates = ReadExcel(dailyRatesPath);

            // Build rate lookup from Daily Rates file using Rate_2
            var rateMap = rates
                .Where(r => r.ContainsKey("BaseCur") && r.ContainsKey("Rate_2"))
                .ToDictionary(
                    r => r["BaseCur"].ToString().Trim(),
                    r => double.TryParse(r["Rate_2"].ToString().Replace(",", ""), out double v) ? v : 0.0
                );

            Console.WriteLine("Filtering relevant contract rows...");
            var filteredBalances = balances
                .Where(r =>
                    r.ContainsKey("ContractNo")
                    && r["ContractNo"].ToString().StartsWith("00070860")
                )
                .ToList();

            Console.WriteLine($"Filtered rows count: {filteredBalances.Count}");

            double usdTotal = 0.0;

            Console.WriteLine("Applying Rate_2 USD conversion...");

            foreach (var row in filteredBalances)
            {
                var baseCur = row.ContainsKey("BaseCurrency") ? row["BaseCurrency"].ToString().Trim() : "";
                var totalBal = row.ContainsKey("TotalBal") &&
                               double.TryParse(row["TotalBal"].ToString().Replace(",", ""), out double bal)
                    ? bal
                    : 0.0;

                if (rateMap.ContainsKey(baseCur) && rateMap[baseCur] > 0)
                {
                    var rate = rateMap[baseCur];
                    double usdValue = Math.Round(totalBal / rate, 2);

                    row["Rate"] = rate;
                    row["USDBal"] = usdValue;

                    usdTotal += usdValue;
                }
                else
                {
                    row["Rate"] = "";
                    row["USDBal"] = "";
                }
            }

            Console.WriteLine($"Total USD Balance = {usdTotal}");

            Console.WriteLine("Writing CSV file with total...");
            WriteCsv(outputCsvPath, filteredBalances, usdTotal);

            return usdTotal; // <-- IMPORTANT
        }

        // Writes main conversion output
        private void WriteCsv(string path, List<Dictionary<string, object>> rows, double totalUsd)
        {
            using var sw = new StreamWriter(path);

            var headers = rows.First().Keys.ToList();

            if (!headers.Contains("Rate")) headers.Add("Rate");
            if (!headers.Contains("USDBal")) headers.Add("USDBal");

            sw.WriteLine(string.Join(",", headers));

            foreach (var row in rows)
            {
                sw.WriteLine(string.Join(",", headers.Select(h => row.ContainsKey(h) ? row[h] : "")));
            }

            sw.WriteLine("");
            sw.WriteLine($",,,,,,,,,,,,,,,,,,,,,,,,,,,TOTAL_USD_BAL,{totalUsd}");
        }

        // NEW — writes the Multicurr CSV
        public void WriteMulticurrCsv(string folderPath, double usdTotal)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "Multicurr.csv");

            // Total columns needed: at least 18 to include R
            var columns = new List<string>();

            // Fill columns 1–3
            columns.Add("IMRW");                        // Col 1
            columns.Add("20970443506085");              // Col 2
            columns.Add("MASTERCARD GENERAL USD POOL"); // Col 3

            // Fill columns 4–11 with empty strings
            for (int i = 0; i < 8; i++)
                columns.Add(""); // Cols 4–11

            columns.Add("Balance_bank");                // Col 12 (L)
            columns.Add("11/30/2025");                  // Col 13 (M)

            // Fill columns 14–16 empty
            for (int i = 0; i < 3; i++)
                columns.Add(""); // Cols 14–16

            columns.Add(usdTotal.ToString("F2"));      // Col 17 (Q)
            columns.Add("USD");                         // Col 18 (R)

            // Join with commas for proper CSV
            string line = string.Join(",", columns);

            File.WriteAllText(filePath, line);
        }



        // Excel reader
        private List<Dictionary<string, object>> ReadExcel(string path)
        {
            var list = new List<Dictionary<string, object>>();

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            IWorkbook workbook = Path.GetExtension(path).Equals(".xls", StringComparison.OrdinalIgnoreCase)
                ? new HSSFWorkbook(fs)
                : new XSSFWorkbook(fs);

            var sheet = workbook.GetSheetAt(0);
            var headerRow = sheet.GetRow(0);

            var headers = Enumerable.Range(0, headerRow.LastCellNum)
                .Select(i => headerRow.GetCell(i)?.ToString() ?? $"Column{i}")
                .ToList();

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var dict = new Dictionary<string, object>();
                for (int j = 0; j < headers.Count; j++)
                {
                    dict[headers[j]] = row.GetCell(j)?.ToString() ?? "";
                }

                list.Add(dict);
            }

            return list;
        }
    }
}
