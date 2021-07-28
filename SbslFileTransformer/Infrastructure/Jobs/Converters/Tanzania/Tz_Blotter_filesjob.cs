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
    public class Tz_Blotter_filesjob : ConverterJobBase<Tz_Blotter_filesjob>, IHostedService
    {
        public Tz_Blotter_filesjob(ILogger<Tz_Blotter_filesjob> logger, IServiceScopeFactory serviceScopeFactory,
           EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting ATM Journal Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertBlotterFile(), null,
                TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }

        private async Task ConvertBlotterFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running TZ Blotter Converter Job");

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

                    //var files = Directory.GetFiles(prodFolder, "*.xlsx", options).ToList();

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx"))
                    .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xlsx")));


                    var Blotter_Converter = new Tz_Blotter_Converter();

                    foreach (var file in files)
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.ToLower().Contains("treasury_accounts") && file.ToLower().Contains("blotter") && file.ToLower().Contains("imtz"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());
                        if (fileToProcess != null && fileToProcess.Converted == false)
                           try
                            {
                                Blotter_Converter.Convert_Blotter_file(file);
                                    fileToProcess.Converted = true;
                                }
                            catch (Exception ex)
                            {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                var archive = "";

                                archive = Path.Combine(Path.GetDirectoryName(file) + "\\blotter", "FAILED",
                                    DateTime.Now.ToString("yyMMdd") + "\\blotter");
                                if (!Directory.Exists(archive))
                                    Directory.CreateDirectory(archive);


                                try
                                {
                                    File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + ".err");
                                    File.Delete(file);
                                }
                                catch (Exception xc)
                                {
                                }

                                await EmailHelpers.SendEmails(dbContext, "Error in Blotter file conversion",
                                    $"Problem with  file {file} \n\n {ex.Message}", new[] { file }, _emailSender);
                            }
                            finally
                            {
                                    

                                    fileToProcess.ConvertedBy = nameof(Tz_Blotter_Converter);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();

                                    var archive = "";

                                archive = Path.Combine(Path.GetDirectoryName(file), "ARCHIVE",
                                    DateTime.Now.ToString("yyMMdd"));
                                if (!Directory.Exists(archive))
                                    Directory.CreateDirectory(archive);


                                try
                                {
                                    File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + ".blt");
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
