using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RWMulticurrency_Converter
{
    public class RWMulticurrencyBackgroundJob : BackgroundService
    {
        private readonly ILogger<RWMulticurrencyBackgroundJob> _logger;
        private readonly RWMulticurrencyConverter _converter;

        public RWMulticurrencyBackgroundJob(
            ILogger<RWMulticurrencyBackgroundJob> logger,
            RWMulticurrencyConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RW Multicurrency Background Job started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running scheduled RW multicurrency conversion...");

                    // Define environment paths
                    string prodRoot = @"H:\Recon_Files\IMRW\CARDS\MC_GNR_POOL";                   // Production
                    string clientTestRoot = @"C:\Users\SFTP\Desktop\Recon_Files\IMRW\CARDS\MC_GNR_POOL";  // Client test
                    string localTestRoot = @"C:\Users\OduorTimothy\Folder\Production\IMRW\CARDS\MC_GNR_POOL"; // Your local test

                    // Determine which root exists (priority: production → client test → local test)
                    string rootDir = Directory.Exists(prodRoot) ? prodRoot :
                                     Directory.Exists(clientTestRoot) ? clientTestRoot :
                                     localTestRoot;

                    _logger.LogInformation($"Using root directory: {rootDir}");

                    // Folder structure
                    string dailyRatesDir = Path.Combine(rootDir, "Daily_Rates");
                    string refinedDailyDir = Path.Combine(rootDir, "Refined_Daily_Rates");
                    string contractDir = Path.Combine(rootDir, "NewNI_Contract_File");
                    string convertedDir = Path.Combine(contractDir, "Converted");

                    // Ensure all required folders exist
                    foreach (var folder in new[] { dailyRatesDir, refinedDailyDir, contractDir, convertedDir })
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                            _logger.LogInformation($"Created missing folder: {folder}");
                        }
                    }


                    // Get contract files
                    var contractFiles = Directory.GetFiles(contractDir, "*.xlsx")
                        .Concat(Directory.GetFiles(contractDir, "*.xls"))
                        .ToArray();

                    if (contractFiles.Length == 0)
                    {
                        _logger.LogInformation("No contract files found.");
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                        continue;
                    }

                    // Get refined rate files
                    var rateFiles = Directory.GetFiles(refinedDailyDir, "*.xlsx");
                    if (rateFiles.Length == 0)
                    {
                        _logger.LogWarning($"No refined daily rate files found in {refinedDailyDir}");
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                        continue;
                    }

                    foreach (var contractFile in contractFiles)
                    {
                        // Skip if file is locked/in use
                        if (IsFileLocked(contractFile))
                        {
                            _logger.LogWarning($"Contract file is in use, skipping: {contractFile}");
                            continue;
                        }

                        string rateFile = rateFiles.FirstOrDefault(r =>
                            Path.GetFileNameWithoutExtension(r)
                                .Contains(Path.GetFileNameWithoutExtension(contractFile)))
                            ?? rateFiles[0];

                        string outputFileName = Path.GetFileNameWithoutExtension(contractFile) + "_Converted.csv";
                        string outputPath = Path.Combine(convertedDir, outputFileName);

                        _logger.LogInformation($"Converting {contractFile} using {rateFile}");

                        double usdTotal = _converter.Convert(contractFile, rateFile, outputPath);
                        _logger.LogInformation($"Conversion completed: {outputPath}");

                        string prodFolder = @"H:\Recon_Files";
                        string localFallback = @"C:\Temp\Recon_Files_Test";

                        // Write to local fallback
                        try
                        {
                            if (!Directory.Exists(localFallback))
                                Directory.CreateDirectory(localFallback);

                            _converter.WriteMulticurrCsv(localFallback, usdTotal);
                            _logger.LogInformation($"Multicurr CSV written to local fallback: {localFallback}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to write to local fallback '{localFallback}': {ex.Message}");
                        }

                        // Write to network/prod folder safely
                        try
                        {
                            if (!Directory.Exists(prodFolder))
                                Directory.CreateDirectory(prodFolder);

                            _converter.WriteMulticurrCsv(prodFolder, usdTotal);
                            _logger.LogInformation($"Multicurr CSV written to prod folder: {prodFolder}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Could not write to prod folder '{prodFolder}': {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RW Multicurrency conversion job.");
                }

                // Wait 5 minutes before next iteration
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        // Helper to detect if a file is locked/in use
        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
