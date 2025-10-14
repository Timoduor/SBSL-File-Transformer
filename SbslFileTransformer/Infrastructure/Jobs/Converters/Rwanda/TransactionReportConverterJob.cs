using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.BalanceExtractors.Rwanda;
using SbslFileTransformer.Converters.Rwanda;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Rwanda
{
    public class TransactionReportConverterJob : ConverterJobBase<TransactionReportConverterJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(TransactionReportConverterJob);

        public TransactionReportConverterJob(ILogger<TransactionReportConverterJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
#if DEBUG
            Console.WriteLine("🟢 [DEBUG] TransactionReportConverterJob: StartAsync() triggered.");
#endif
            this._logger.LogInformation("Starting Spenn RW Balance Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.AirtelFileBalanceExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task AirtelFileBalanceExtractor()
        {
#if DEBUG
            Console.WriteLine("🟣 [DEBUG] AirtelFileBalanceExtractor() running...");
#endif
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running Spenn RW Balance Extractor job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
#if DEBUG
                    Console.WriteLine("🔍 [DEBUG] Fetching ApplicationDbContext...");
#endif
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Configuration> configurations = dbContext.Configurations.ToList();

#if DEBUG
                    Console.WriteLine($"✅ [DEBUG] Configurations loaded: {configurations.Count}");
#endif

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

#if DEBUG
                    Console.WriteLine($"📁 [DEBUG] prodFolder: {prodFolder}");
                    Console.WriteLine($"📁 [DEBUG] sbFolder: {sbFolder}");
                    Console.WriteLine($"🏢 [DEBUG] Entity: {Entity}");
#endif

                    // --- Rwanda Excel-to-CSV Conversion Integration Start ---
                    using (var rwscope = _serviceScopeFactory.CreateScope())
                    {
                        var converter = rwscope.ServiceProvider.GetRequiredService<RWExcelToCSVConverter>();

                        string fxMarkupFolder = Path.Combine(prodFolder, "IMRW", "CARDS", "MC_GNR_POOL", "PORTAL", "FxMarkup");
                        string trxDumpFolder = Path.Combine(prodFolder, "IMRW", "CARDS", "MC_GNR_POOL", "PORTAL", "TrxDump");

                        string fxMarkupConverted = Path.Combine(fxMarkupFolder, "CONVERTED");
                        string trxDumpConverted = Path.Combine(trxDumpFolder, "CONVERTED");

#if DEBUG
                        Console.WriteLine("🧠 [DEBUG] Preparing to convert Rwanda Excel files...");
                        Console.WriteLine($"➡️ FxMarkup folder: {fxMarkupFolder}");
                        Console.WriteLine($"➡️ TrxDump folder: {trxDumpFolder}");
#endif

                        _logger.LogInformation("[RW Converter] Starting Rwanda Excel-to-CSV conversion...");

                        try
                        {
                            var fxMarkupFiles = await converter.ConvertExcelFilesAsync(fxMarkupFolder, fxMarkupConverted);
#if DEBUG
                            Console.WriteLine($"✅ [DEBUG] FxMarkup conversion complete. Files converted: {fxMarkupFiles?.Count ?? 0}");
#endif

                            var trxDumpFiles = await converter.ConvertExcelFilesAsync(trxDumpFolder, trxDumpConverted);
#if DEBUG
                            Console.WriteLine($"✅ [DEBUG] TrxDump conversion complete. Files converted: {trxDumpFiles?.Count ?? 0}");
#endif
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            Console.WriteLine($"❌ [DEBUG] Rwanda Converter failed: {ex}");
#endif
                            _logger.LogError(ex, "[RW Converter] Conversion error occurred!");
                        }
                    }
                    // --- Rwanda Excel-to-CSV Conversion Integration End ---

#if DEBUG
                    Console.WriteLine("🧾 [DEBUG] Proceeding to file scanning logic...");
#endif

                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options));

#if DEBUG
                    Console.WriteLine($"📂 [DEBUG] Total files found: {files.Count}");
#endif

                    TransactionReportConverter mpesaConverter = new TransactionReportConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();
                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("cards") && file.ToLower().Contains("imrw") &&
                            ((file.ToLower().Contains("office_accounts") && file.ToLower().Contains("mastercard_file")) ||
                             (file.ToLower().Contains("mc_gnr_pool") && file.ToLower().Contains("portal"))) &&
                            !file.ToLower().Contains("conv"))
                        {
#if DEBUG
                            Console.WriteLine($"🧩 [DEBUG] Processing file: {file}");
#endif
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    bool isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ?? false.ToString());

                                    string rootFolder = isProd ? prodFolder : sbFolder;

                                    mpesaConverter.ConvertFile(file);
#if DEBUG
                                    Console.WriteLine($"✅ [DEBUG] File converted successfully: {file}");
#endif
                                }
                                catch (Exception ex)
                                {
#if DEBUG
                                    Console.WriteLine($"❌ [DEBUG] File processing failed: {file}, Error: {ex}");
#endif
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(SpennRwandaBalanceExtractor));
                                }
                        }
                    }

#if DEBUG
                    Console.WriteLine($"💾 [DEBUG] Saving processed file statuses...");
#endif

                    await this.SaveProcessedFilesStatuses(dbContext, updatedFiles);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"🔥 [DEBUG] Fatal error in AirtelFileBalanceExtractor(): {ex}");
#endif
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
