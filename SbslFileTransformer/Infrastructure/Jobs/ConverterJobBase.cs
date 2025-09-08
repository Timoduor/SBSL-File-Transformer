using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public abstract class ConverterJobBase<T>
    {
        protected static SemaphoreSlim _semaphore;
        protected EmailSender _emailSender;
        protected ILogger<T> _logger;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected IHttpClientFactory HttpClientFactory;
        protected Timer _timer;
        protected string _entity;
        protected abstract string JobName { get; set; }
        protected int RunInterval { get; set; } = 10; //in minutes

        //to specify any extra validations
        protected Predicate<string> FileMeetsConditions { get; set; }

        protected string RequiredPath { get; set; }
        protected List<string> FileExts { get; set; }

        protected string Entity { get; set; }

        protected JobDisplayManager _jobManager;
        protected JobStatus CurrentJobStatus;

        //this method should be abstract to force the actual specific implementation per job
        public virtual async Task ProcessFileAsync(string filePath)
        {
            //code for call the converter goes here
            await Task.CompletedTask;
        }

        //default predicate check it has any of the required extensions and it is in the required path
        private bool FilePathCheck(string file)
        {
            return FileExts.Any(f => file.Contains(f)) && File.Exists(file) && file.ToLower().Contains(RequiredPath.ToLower());
        }

        public virtual async Task RunJob()
        {
            try
            {
                await _semaphore.WaitAsync();

                if (!ValidateJobInputParams(out var missingMessage))
                {
                    throw new MissingFieldException($"{missingMessage}");
                }

                _logger.LogInformation($"Running {JobName} job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    var options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.*", options)
                                    .Where(f => FileExts.Any(e => Path.GetExtension(f).ToLower() == e.ToLower()))
                                    .ToList();

                    foreach (var file in files)
                    {
                        if (!FilePathCheck(file))
                        {
                            return;
                        }

                        if (FileMeetsConditions != null && !FileMeetsConditions(file))
                        {
                            return;
                        }

                        await ProcessFileAsync(file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _ = _semaphore.Release();
            }
        }

        private bool ValidateJobInputParams(out string missingMessage)
        {
            var isValid = true;
            missingMessage = string.Empty;

            if (string.IsNullOrEmpty(JobName))
            {
                missingMessage += "JobName is required, ";
                isValid = false;
            }
            if (string.IsNullOrEmpty(RequiredPath))
            {
                missingMessage += "Required folder path is not provided, ";
                isValid = false;
            }
            if (FileExts.Count() <= 0)
            {
                missingMessage += "At least one file extension should be provided, ";
                isValid = false;
            }

            return isValid;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            _semaphore.Dispose();
            _timer.Dispose();
            return Task.CompletedTask;
        }

        protected async Task ProcessFileFailure(List<Configuration> configurations, string file, SftpUploadedFile fileToProcess, Exception ex, string header = "")
        {
            fileToProcess.Failed = true;

            _logger.LogError(ex, ex.Message + file.ToUpper());

            await EmailHelpers.SendEmails(configurations, string.IsNullOrEmpty(header) ? "Error in File Conversion" : header,
                $"Problem with  file {file} \n\n {ex.Message}", new[] { file }, _emailSender, _logger);
        }

        protected void CompleteFileProcessing(List<SftpUploadedFile> updatedFiles, SftpUploadedFile fileToProcess, string converter, bool isBalanceExtraction = false)
        {
            fileToProcess.BalanceExtracted = isBalanceExtraction;

            fileToProcess.Converted = true;

            fileToProcess.ConvertedBy = converter;

            updatedFiles.Add(fileToProcess);
        }

        protected async Task SaveProcessedFilesStatuses(ApplicationDbContext dbContext, List<SftpUploadedFile> updatedFiles)
        {
            dbContext.UploadedFiles.UpdateRange(updatedFiles);

            _ = await dbContext.SaveChangesAsync();
        }
    }
}
