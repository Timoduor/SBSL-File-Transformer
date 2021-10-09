using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    public class ATMjournalConverterJob : ConverterJobBase<ATMjournalConverterJob>, IHostedService
    {
        public ATMjournalConverterJob(ILogger<ATMjournalConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
           EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting RW ATMjournal Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await RWConvertATMJournal(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task RWConvertATMJournal()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running RW ATM Journal Converter Job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = await dbContext.Configurations.ToListAsync();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".jrn")).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".jrn")));


                    var files_ = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".log")).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".log")));

                    var ATMJournalConverter = new RW_ATMJournalConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    var updatedFiles = new List<SftpUploadedFile>();

                    foreach (var file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.Contains("ATMs") && file.Contains("E-JRN"))
                        {
                            var fileToProcess = uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());
                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    ATMJournalConverter.ConvertFile_WinkaATMjrn(file);                                    
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(RW_ATMJournalConverter));
                                }
                        }
                    }

                    foreach (var file in files_)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.Contains("ATMs") && file.Contains("E-JRN"))
                        {
                            var fileToProcess = uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());
                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    ATMJournalConverter.ConvertFile_WinkaATMjrn(file);
                                    
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(RW_ATMJournalConverter));
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
