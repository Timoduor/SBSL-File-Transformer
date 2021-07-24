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
    public class MpesaB2CnC2BConverterJob : ConverterJobBase<MpesaB2CnC2BConverterJob>, IHostedService
    {
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
                TimeSpan.FromSeconds(new Random().Next(10, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _semaphore.Dispose();
            _timer.Dispose();
            return Task.CompletedTask;
        }

        private async Task ConvertMpesaB2CnC2BFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running MPesa B2C n C2B converter job");

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

                    var files = Directory.GetFiles(prodFolder, "*.*", options)
                        .Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".csv")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options)
                        .Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".csv")));

                    var mpesaConverter = new MpesaB2CnC2BConverter(Entity);

                    foreach (var file in files)
                        if ((file.ToLower().Contains("mpesa") && !file.ToLower().Contains("lookup") &&
                             !file.ToLower().Contains("lipa") && !file.ToLower().Contains("merchant")
                             || file.ToLower().Contains("bank to till b2c") ||
                             file.ToLower().Contains("banktotillb2c") ||
                             file.ToLower().Contains("mmf") && (file.ToLower().Contains("elma_paybill") ||
                                                                file.ToLower().Contains("omni_paybill") ||
                                                                file.ToLower().Contains("pyt_serv_paybill")))
                            && file.ToLower().Contains("imke") && !file.Contains("Conv"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
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

                                    await EmailHelpers.SendEmails(dbContext,
                                        "Problem Converting MPesa B2C or C2B files", $"{file} \n\n {ex.Message}",
                                        new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(MpesaB2CnC2BConverter);

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