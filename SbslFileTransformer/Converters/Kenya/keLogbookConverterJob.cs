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
using SbslFileTransformer.Converters.Rwanda;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class keLogbookConverterJob : ConverterJobBase<keLogbookConverterJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(keLogbookConverterJob);
        public keLogbookConverterJob(ILogger<keLogbookConverterJob> logger, IServiceScopeFactory serviceScopeFactory,
           EmailSender emailSender)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting KE LogbookConverter Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.LogbookConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task LogbookConverter()
        {

            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running KE LogbookConverter Converter Job");


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

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")).ToList();
                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")));

                    KE_LBookConverter LBookConverter = new KE_LBookConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    string backUpPath = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder")).Value;

                    string archdir = Path.Combine(backUpPath, "archConverted");

                    List<Task> tasks = new List<Task>();

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("imke") && file.ToLower().Contains("logbook_ntsa") && !file.ToLower().Contains("arch") && !file.ToLower().Contains("conv"))
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f => f.FilePath.ToLower() == file.ToLower());
                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                tasks.Add(Task.Factory.StartNew(async () =>
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(archdir);

                                        string pdfFile_ = System.IO.Path.GetDirectoryName(file) + "\\" + System.IO.Path.GetFileNameWithoutExtension(file) + ".pdf";

                                        LBookConverter.Removelinebreaks(file);

                                        //archive pdf file only after we have converted the equivalent csv file
                                        if (File.Exists(pdfFile_))
                                        {
                                            File.Move(pdfFile_, archdir);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                    }
                                    finally
                                    {
                                        this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(KE_LBookConverter));
                                    }
                                }));
                            }
                        }
                    }

                    await Task.WhenAll(tasks);
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
