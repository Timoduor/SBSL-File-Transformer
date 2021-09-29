using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class MpesaBalanceExtractorJob : ConverterJobBase<MpesaBalanceExtractorJob>, IHostedService
    {
        public MpesaBalanceExtractorJob(ILogger<MpesaBalanceExtractorJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MPesa Balance Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await MpesaFileBalanceExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(10, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task MpesaFileBalanceExtractor()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running MPesa Balance Extractor job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                        .ToList();

                    Entity = dbContext.Configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv"))
                        .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")));

                    var mpesaConverter = new MpesaBalanceExtractor();

                    foreach (var file in files)
                        if (((file.ToLower().Contains("mpesa") && file.Contains("Conv")) || file.ToLower().Contains("credit_receivable")) &&
                            file.ToLower().Contains("imke"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.BalanceExtracted == false)
                                try
                                {
                                    var isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                        false.ToString());

                                    var rootFolder = isProd ? prodFolder : sbFolder;

                                    mpesaConverter.ConvertFile(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting MPesa Balance files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.BalanceExtracted = true;

                                    fileToProcess.ConvertedBy = nameof(MpesaBalanceExtractor);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();
                                }
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}