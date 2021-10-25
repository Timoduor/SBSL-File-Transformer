using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
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
    public class CrdbPdfToMTFileJob : ConverterJobBase<CrdbPdfToMTFileJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(CrdbPdfToMTFileJob);
        public CrdbPdfToMTFileJob(ILogger<CrdbPdfToMTFileJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting CRDB PDF To MT File Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ConvertPdfToMTFile(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertPdfToMTFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running CRDB PDF To MT File converter job");

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

                    var isProd =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value);

                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.pdf", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.pdf", options));

                    var pdfConverter = new CrdbPdfToMTFilesConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    var updatedFiles = new List<SftpUploadedFile>();

                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME CAMT053 SOMEWHERE IN IT
                        if (file.ToLower().Contains("crdb") && file.ToLower().Contains("imtz"))
                        {
                            var fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    var statementFolder = Path.Combine(sbFolder, @$"{Entity}\NOSTRO\STATEMENT");

                                    if (isProd)
                                        statementFolder = Path.Combine(prodFolder, @$"{Entity}\NOSTRO\STATEMENT");


                                    var pdfPassword = await dbContext.Configurations.FirstOrDefaultAsync(c =>
                                        c.ConfigType == ConfigurationType.Setting && c.Key == "PdfPassword");
                                    pdfConverter.ConvertFile(file, pdfPassword?.Value, statementFolder);
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(CrdbPdfToMTFilesConverter));
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