using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
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
    public class EpinConverterJob : ConverterJobBase<EpinConverterJob>, IHostedService
    {
        public EpinConverterJob(ILogger<EpinConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting EPIN Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertEPINFile(), null,
                TimeSpan.FromSeconds(new Random().Next(5, 15)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }

        private async Task ConvertEPINFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running MasterCard converter job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = await dbContext.Configurations
                        .Where(c => c.ConfigType == ConfigurationType.Sftp).ToListAsync();

                    Entity = dbContext.Configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity")?.Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.txt", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.txt", options));

                    var epinConverter = new EpinConverter();

                    foreach (var file in files)
                        if (file.ToLower().Contains("epin") && file.ToLower().Contains("imke") &&
                            file.ToLower().Contains("cards"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    epinConverter.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting EPIN files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(EpinConverter);

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