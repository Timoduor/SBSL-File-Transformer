using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Kenya;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class WesternUnionActivitiesKEConverterJob : ConverterJobBase<WesternUnionActivitiesKEConverterJob>,
        IHostedService
    {
        public WesternUnionActivitiesKEConverterJob(ILogger<WesternUnionActivitiesKEConverterJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Western Union KE Activities Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await WesternUnionActivitiesConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(15, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task WesternUnionActivitiesConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Western Union KE Activities job");

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

                    var files = Directory.GetFiles(prodFolder, "*", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*", options));

                    var westernUnionConverter = new WesternUnionActivitiesKEConverter();

                    foreach (var file in files)
                        if (!file.Contains("Conv") && file.ToLower().Contains("imke") &&
                            file.ToLower().Contains("western_union") && file.ToLower().Contains("activities"))
                        {
                            var fileToProcess =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    westernUnionConverter.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcess.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext,
                                        "Problem Converting Western Union Activities KE files",
                                        $"{file} \n\n {ex.Message}", new[] { file }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;

                                    fileToProcess.ConvertedBy = nameof(WesternUnionActivitiesKEConverter);

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