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
            this._logger.LogInformation("Starting Spenn RW Balance Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.AirtelFileBalanceExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task AirtelFileBalanceExtractor()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running Spenn RW Balance Extractor job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    // --- Rwanda Excel-to-CSV Conversion Integration Start ---
                    using (var rwscope = _serviceScopeFactory.CreateScope())
                    {
                        var converter = rwscope.ServiceProvider.GetRequiredService<RWExcelToCSVConverter>();

                        string fxMarkupFolder = Path.Combine(prodFolder, "IMRW", "CARDS", "MC_GNR_POOL", "PORTAL", "FxMarkup");
                        string trxDumpFolder = Path.Combine(prodFolder, "IMRW", "CARDS", "MC_GNR_POOL", "PORTAL", "TrxDump");

                        string fxMarkupConverted = Path.Combine(fxMarkupFolder, "CONVERTED");
                        string trxDumpConverted = Path.Combine(trxDumpFolder, "CONVERTED");

                        _logger.LogInformation("[RW Converter] Starting Rwanda Excel-to-CSV conversion...");

                        try
                        {
                            var fxMarkupFiles = await converter.ConvertExcelFilesAsync(fxMarkupFolder, fxMarkupConverted);
                            var trxDumpFiles = await converter.ConvertExcelFilesAsync(trxDumpFolder, trxDumpConverted);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[RW Converter] Conversion error occurred!");
                        }
                    }
                    // --- Rwanda Excel-to-CSV Conversion Integration End ---

                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options));

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
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    bool isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ?? false.ToString());

                                    string rootFolder = isProd ? prodFolder : sbFolder;

                                    mpesaConverter.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(SpennRwandaBalanceExtractor));
                                }
                        }
                    }

                    await this.SaveProcessedFilesStatuses(dbContext, updatedFiles);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
