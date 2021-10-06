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
    public class PrepaidCardbalanceJob : ConverterJobBase<PrepaidCardbalanceJob>, IHostedService
    {

        public PrepaidCardbalanceJob(ILogger<PrepaidCardbalanceJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _semaphore.Dispose();
            _timer.Dispose();
            return Task.CompletedTask;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Prepaid Authorization report Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await Prepaidbalance_Converter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 120)), TimeSpan.FromMinutes(10));

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

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                        .ToList();

                    Entity = dbContext.Configurations
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

                    foreach (var file in files)
                        //IMTZ\CARDS_ATM\FloatCMS\Balances
                        if (file.ToLower().Contains("imtz") && file.ToLower().Contains("cards_atm") && file.ToLower().Contains("floatcms") && file.ToLower().Contains("balances"))//&& !file.ToLower().Contains("conv")
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    Prepaidbal.ConvertFile(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting  Prepaid Authorization report  files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(PrepaidBal_converter);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();
                                    var archive = "";

                                    archive = Path.Combine(Path.GetDirectoryName(file), "ARCHIVE",
                                        DateTime.Now.ToString("yyMMdd"));
                                    if (!Directory.Exists(archive))
                                        Directory.CreateDirectory(archive);


                                    try
                                    {
                                        File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".cms");
                                        File.Delete(file);
                                    }
                                    catch (Exception xc)
                                    {
                                        _logger.LogError(xc, xc.Message);
                                    }

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
