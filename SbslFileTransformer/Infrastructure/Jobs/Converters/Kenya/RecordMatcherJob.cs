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
    public class RecordMatcherJob : ConverterJobBase<RecordMatcherJob>, IHostedService
    {
        public RecordMatcherJob(ILogger<RecordMatcherJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Record Matcher Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await RecordMatcherExtractorConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(15, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task RecordMatcherExtractorConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Record Matcher Extractor job");

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

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".xlsx"))
                       .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".xlsx")));

                    var mpesaConverter = new RecordMatcherExtractor();

                    foreach (var file in files)
                    {
                        if (file.ToLower().Contains("cards") && file.ToLower().Contains("credit_card")
                            && file.ToLower().Contains("collections_gl") && file.ToLower().Contains("imke"))
                        {
                            var fileToProcessGL =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            var glFilePath = fileToProcessGL.FilePath;
                            var cmsFilePath = Path.ChangeExtension(glFilePath.Replace("Collections_GL", "Collections_CMS"), ".xls");

                            var fileToProcessCMS =
                                await dbContext.UploadedFiles.FirstOrDefaultAsync(f =>
                                    f.FilePath.ToLower() == cmsFilePath.ToLower());

                            if (!File.Exists(cmsFilePath))
                            {
                                await EmailHelpers.SendEmails(dbContext, $"Missing Expected CMS file {cmsFilePath}",
                                    $"Expected {cmsFilePath} but was not found for balance check. Please place the file with a matching Collections_GL file",
                                    new[] { file }, _emailSender);
                                continue;
                            }

                            if (!File.Exists(glFilePath))
                            {
                                await EmailHelpers.SendEmails(dbContext, $"Missing Expected GL file {glFilePath}",
                                    $"Expected {glFilePath} but was not found for balance check. Please place the file with a matching Collections_CMS file",
                                    new[] { file }, _emailSender);
                                continue;
                            }

                            if (fileToProcessGL != null && fileToProcessGL.Converted == false
                                && fileToProcessCMS != null && fileToProcessCMS.Converted == false)
                            {

                                string path = Path.GetDirectoryName(glFilePath);
                                string outputPath = Path.Combine(Path.GetFullPath(Path.Combine(path, @"..\")), "Conv");

                                Directory.CreateDirectory(outputPath);

                                try
                                {
                                    mpesaConverter.MatchFiles(glFilePath, cmsFilePath, outputPath);
                                }
                                catch (Exception ex)
                                {
                                    fileToProcessGL.Failed = true;
                                    fileToProcessCMS.Failed = true;

                                    _logger.LogError(ex, ex.Message);

                                    await EmailHelpers.SendEmails(dbContext, "Problem Converting Omni Lookup files",
                                        $"{glFilePath},{cmsFilePath} \n\n {ex.Message}", new[] { glFilePath, cmsFilePath }, _emailSender);
                                }
                                finally
                                {
                                    fileToProcessGL.Converted = true;
                                    fileToProcessCMS.Converted = true;

                                    fileToProcessGL.ConvertedBy = nameof(RecordMatcherExtractor);
                                    fileToProcessCMS.ConvertedBy = nameof(RecordMatcherExtractor);

                                    dbContext.Update(fileToProcessGL);
                                    dbContext.Update(fileToProcessCMS);

                                    await dbContext.SaveChangesAsync();
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _semaphore.Dispose();
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
