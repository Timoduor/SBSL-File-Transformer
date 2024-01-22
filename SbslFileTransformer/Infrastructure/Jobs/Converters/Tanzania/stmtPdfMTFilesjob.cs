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
using SbslFileTransformer.Converters.Tanzania.TzPDF;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Tanzania
{
    public class stmtPdfMTFilesjob : ConverterJobBase<stmtPdfMTFilesjob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(DtbPdfToMTFileJob);
        public stmtPdfMTFilesjob(ILogger<stmtPdfMTFilesjob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting TISS STATEMENT Converter Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.ConvertPdfToMTFile(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ConvertPdfToMTFile()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running TISS STATEMENT  converter job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    bool isProd =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value);

                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.pdf", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.pdf", options));

                    genStatemenPdfToMTFilesConverter pdfConverter = new genStatemenPdfToMTFilesConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                 
                        if (file.ToLower().Contains("imtz") && file.ToLower().Contains("nostro") && file.ToLower().Contains("bot usd") || file.ToLower().Contains("bot tzs"))
                        {
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());
                          
                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    //string statementFolder = Path.Combine(sbFolder, @$"{Entity}\BOT TZS"); bot usd

                                    //if (isProd)
                                    //    statementFolder = Path.Combine(prodFolder, @$"{Entity}\BOT TZS");


                                    //Configuration pdfPassword = await dbContext.Configurations.FirstOrDefaultAsync(c =>
                                    //    c.ConfigType == ConfigurationType.Setting && c.Key == "PdfPassword");
                                    pdfConverter.ConvertFile_Tiss(file, "");
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    fileToProcess.Converted = true;
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(ConvertPdfToMTFile));
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
