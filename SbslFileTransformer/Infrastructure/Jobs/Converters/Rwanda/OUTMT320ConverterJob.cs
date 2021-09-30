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
    public class OUTMT320ConverterJob : ConverterJobBase<OUTMT320ConverterJob>, IHostedService
    {
        public OUTMT320ConverterJob(ILogger<OUTMT320ConverterJob> logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting OUTMT320 Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertOutMT320File(), null, TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertOutMT320File()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running OUTMT300 Converter Job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = await dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToListAsync();

                    Entity = dbContext.Configurations.FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.TXT", options).ToList();



                    var mt320Converter = new OUTMt320Converter();

                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.ToLower().Contains("mt320") && file.ToLower().Contains("imrw"))
                        {
                            var fileToProcess = await dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    mt320Converter.ProcessOutMt320File(file);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(OUTMT300ConverterJob);

                                    dbContext.Update(fileToProcess);

                                    await dbContext.SaveChangesAsync();

                                    _logger.LogError(ex, ex.Message);

                                    string archive = "";

                                    archive = Path.Combine(Path.GetDirectoryName(file) + "\\MT320", "FAILED", DateTime.Now.ToString("yyMMdd") + "\\RTGSMT300");
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
                                    await EmailHelpers.SendEmails(dbContext, "Error in  OUTMT300 file conversion", $"Problem with  file {file} \n\n {ex.Message}", new string[] { file }, _emailSender);
                                }
                                finally
                                {


                                    string archive = "";

                                    archive = Path.Combine(Path.GetDirectoryName(file) + "\\MT320", "ARCHIVE", DateTime.Now.ToString("yyMMdd") + "\\RTGSMT300");
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
