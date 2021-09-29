using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Rwanda.Camt053;
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
    public class MT300RWConverterJob : ConverterJobBase<MT300RWConverterJob>, IHostedService
    {
        public MT300RWConverterJob(ILogger<MT300RWConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MT300 Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertMT300File(), null,
                TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertMT300File()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running MT300 Converter Job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = await dbContext.Configurations
                        .Where(c => c.ConfigType == ConfigurationType.Sftp).ToListAsync();

                    Entity = dbContext.Configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };


                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".bkp")).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".bkp")));

                    var mt300Converter = new Mt300Converter();

                    foreach (var file in files)
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.ToLower().Contains("imrw") && file.ToLower().Contains("treasury") && file.ToLower().Contains("fx_confirmation") && file.ToLower().Contains("mt300"))
                        {
                            var fileToProcess = await dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.FilePath.ToLower() == file.ToLower());
                            var outputfile = Path.GetDirectoryName(file) + "\\Converted_MT300_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".csv";

                            if (fileToProcess != null && fileToProcess.Converted == false)

                                try
                                {
                                    mt300Converter.ProcessMt300File(file);
                                    fileToProcess.Converted = true;
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    var archive = "";

                                    archive = Path.Combine(Path.GetDirectoryName(file) + "\\MT300", "FAILED",
                                        DateTime.Now.ToString("yyMMdd") + "\\RTGSMT300");
                                    if (!Directory.Exists(archive))
                                        Directory.CreateDirectory(archive);


                                    try
                                    {
                                        File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + ".out");
                                        File.Delete(file);
                                    }
                                    catch (Exception xc)
                                    {
                                    }

                                    await EmailHelpers.SendEmails(dbContext, "Error in MT300 file conversion", $"Problem with  file {file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {


                                    fileToProcess.ConvertedBy = nameof(MT300RWConverterJob);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();


                                }
                            //}
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