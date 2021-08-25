using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.CDM;
using SbslFileTransformer.Converters.Rwanda;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class FxposfcdailyJob : ConverterJobBase<FxposfcdailyJob>, IHostedService
    {

        public FxposfcdailyJob(ILogger<FxposfcdailyJob> logger, IServiceScopeFactory serviceScopeFactory,
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
            _logger.LogInformation("Starting Fxposfcdaily  Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await FxposfcdailyConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task FxposfcdailyConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Fxposfcdaily Converter RW job");

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

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx")));

                    var fc_Converter = new fc_dailyConverter ();

                    foreach (var file in files)
                        //SPECIFY FOLDER and file extension above PENDING
                        //imrw/treasury/fx_pos/fx_pos_fc_daily
                        if (file.ToLower().Contains("imrw") && file.ToLower().Contains("treasury") && file.ToLower().Contains("fx_pos") && file.ToLower().Contains("fx_pos_fc_daily"))//&& !file.ToLower().Contains("conv")
                        {
                            var fileToProcess = await dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    fc_Converter.ConvertFile(file);
                                    fileToProcess.Converted = true;
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting  fx_pos_ft_sum files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    

                                    fileToProcess.ConvertedBy = nameof(fc_dailyConverter);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();
                                    var archive = "";

                                    archive = Path.Combine(Path.GetDirectoryName(file), "ARCHIVE",
                                        DateTime.Now.ToString("yyMMdd"));
                                    if (!Directory.Exists(archive))
                                        Directory.CreateDirectory(archive);


                                    try
                                    {
                                        File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".fx");
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
