using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class MpesaB2CnC2BConverterJob : ConverterJobBase<MpesaB2CnC2BConverterJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(MpesaB2CnC2BConverterJob);
        public MpesaB2CnC2BConverterJob(ILogger<MpesaB2CnC2BConverterJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MPesa B2C Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertMpesaB2CnC2BFile(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertMpesaB2CnC2BFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running MPesa B2C n C2B converter job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options)
                        .Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".csv")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options)
                        .Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".csv")));

                    MpesaB2CnC2BConverter mpesaConverter = new MpesaB2CnC2BConverter(Entity);

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                        if ((file.ToLower().Contains("mpesa") && !file.ToLower().Contains("lookup") &&
                             !file.ToLower().Contains("lipa") && !file.ToLower().Contains("merchant")
                             || file.ToLower().Contains("bank to till b2c") ||
                             file.ToLower().Contains("banktotillb2c") || file.ToLower().Contains("credit_receivable") ||
                             file.ToLower().Contains("mmf") && (file.ToLower().Contains("elma_paybill") ||
                                                                file.ToLower().Contains("omni_paybill") ||
                                                                file.ToLower().Contains("pyt_serv_paybill")))
                            && file.ToLower().Contains("imke") && !file.Contains("Conv"))
                        {
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    bool isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                        false.ToString());

                                    string rootFolder = isProd ? prodFolder : sbFolder;

                                    mpesaConverter.ConvertFile(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(MpesaB2CnC2BConverter));
                                }
                        }
                    }
                    await SaveProcessedFilesStatuses(dbContext, updatedFiles);
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