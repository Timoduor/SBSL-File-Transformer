using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
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
    public class PrepaidCardbalanceJob : ConverterJobBase<PrepaidCardbalanceJob>, IHostedService
    {

        public PrepaidCardbalanceJob(ILogger<PrepaidCardbalanceJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Prepaid Authorization report Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await Prepaidbalance_Converter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task Prepaidbalance_Converter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Prepaid Authorization report TZ job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    var isProd = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ?? false.ToString());
                    var rootFolder = isProd ? prodFolder : sbFolder;

                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")));

                    var Prepaidbal = new PrepaidBal_converter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    var updatedFiles = new List<SftpUploadedFile>();

                    foreach (var file in files)
                    {
                        //IMTZ\CARDS_ATM\FloatCMS\Balances
                        if (file.ToLower().Contains("imtz") && file.ToLower().Contains("cards_atm") && file.ToLower().Contains("floatcms") && file.ToLower().Contains("balances"))//&& !file.ToLower().Contains("conv")
                        {
                            var fileToProcess =
                                uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    Prepaidbal.ConvertFile(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, typeof(PrepaidBal_converter));
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
