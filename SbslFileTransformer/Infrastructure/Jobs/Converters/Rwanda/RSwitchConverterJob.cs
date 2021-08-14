using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Rwanda;
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
    public class RSwitchConverterJob : ConverterJobBase<RSwitchConverterJob>, IHostedService
    {
        public RSwitchConverterJob(ILogger<RSwitchConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting RSwitch Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await RSwitchConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task RSwitchConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running RSwitch Converter RW job");

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

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".txt"))
                        .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".txt")));

                    var rSwitchConverter_ = new RSwitchConverter();

                    foreach (var file in files)
                        //SPECIFY FOLDER and file extension above PENDING

                        if (file.Contains("ATMs-OFF-US") && file.Contains("IMRW") && !file.Contains("Conv"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    if (file.Contains("Member_DailyAcquiringATMTrxRpt")) rSwitchConverter_.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting  Rswitch files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(RSwitchConverterJob);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();
                                    var archive = "";

                                    archive = Path.Combine(Path.GetDirectoryName(file), "ARCHIVE",
                                        DateTime.Now.ToString("yyMMdd"));
                                    if (!Directory.Exists(archive))
                                        Directory.CreateDirectory(archive);


                                    try
                                    {
                                        File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".rsw");
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _semaphore.Dispose();
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
