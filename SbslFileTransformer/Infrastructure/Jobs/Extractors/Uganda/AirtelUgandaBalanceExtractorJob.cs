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
using SbslFileTransformer.Converters.BalanceExtractors.Uganda;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Extractors.Uganda
{
    public class AirtelUgandaBalanceExtractorJob : ConverterJobBase<GLBalanceExtractorJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(AirtelUgandaBalanceExtractorJob);

        public AirtelUgandaBalanceExtractorJob(IServiceScopeFactory serviceScopeFactory, ILogger<GLBalanceExtractorJob> logger, JobDisplayManager jobManager)
        {
            this._serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
            this._jobManager = jobManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._timer = new Timer(async state => await this.ExtractMultiCurrBalances(), null,
                TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        private async Task ExtractMultiCurrBalances()
        
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running Airtel Uganda Balance Extractor job");

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


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv"))
                        .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")));

                    AirtelUgandaBalanceExtractor mpesaConverter = new AirtelUgandaBalanceExtractor();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("imug") && file.ToLower().Contains("mobile_banking") && file.ToLower().Contains("airtel") &&  file.ToLower().Contains("w2b"))
                        {
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.BalanceExtracted == false)
                                try
                                {
                                    bool isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                        false.ToString());

                                    string rootFolder = isProd ? prodFolder : sbFolder;

                                    mpesaConverter.ConvertFile(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    fileToProcess.BalanceExtracted = true;

                                    fileToProcess.ConvertedBy = nameof(AirtelUgandaBalanceExtractor);

                                    updatedFiles.Add(fileToProcess);
                                }
                        }
                        else if ( file.ToLower().Contains("imug") && file.ToLower().Contains("mobile_banking") && file.ToLower().Contains("airtel") && file.ToLower().Contains("b2w"))
                        {
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.BalanceExtracted == false)
                                try
                                {
                                    bool isProd = Convert.ToBoolean(
                                        configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                        false.ToString());

                                    string rootFolder = isProd ? prodFolder : sbFolder;

                                    mpesaConverter.ConvertFile_B2W(file, rootFolder);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    fileToProcess.BalanceExtracted = true;

                                    fileToProcess.ConvertedBy = nameof(AirtelUgandaBalanceExtractor);

                                    updatedFiles.Add(fileToProcess);
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
