using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SbslFileTransformer.Converters.Kenya;
using SbslFileTransformer.Converters.Rwanda;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{

    public class keATMConverterJob : ConverterJobBase<keATMConverterJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(keATMConverterJob);
        public keATMConverterJob(ILogger<keATMConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
           EmailSender emailSender)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting KE ATMjournal Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.RWConvertATMJournal(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task RWConvertATMJournal()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running KE ATM Journal Converter Job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Configuration> configurations = await dbContext.Configurations.ToListAsync();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".jrn")).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".jrn")));


                    List<string> files_ = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".log")).ToList();
                    files_.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".log")));

                    KE_ATMJournalConverter ATMJournalConverter = new KE_ATMJournalConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.ToLower().Contains("atms") && file.ToLower().Contains("e-jrn") && file.ToLower().Contains("imke"))
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());
                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    ATMJournalConverter.ConvertFile_WinkaATMjrn(file);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(KE_ATMJournalConverter));
                                }
                        }
                    }

                    foreach (string file in files_)
                    {
                        //FILE PATH SHOULD HAVE FOLDER NAME MT300 SOMEWHERE IN IT
                        if (file.ToLower().Contains("atms") && file.ToLower().Contains("e-jrn") && file.ToLower().Contains("imke"))
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());
                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    ATMJournalConverter.ConvertFile_NCR(file);
                                    //ATMJournalConverter.ConvertFile_WinkaATMjrn(file);

                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(RW_ATMJournalConverter));
                                }
                        }
                    }

                    await this.SaveProcessedFilesStatuses(dbContext, updatedFiles);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
