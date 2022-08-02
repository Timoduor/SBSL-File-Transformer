using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Infrastructure.Jobs.Others
{
    public class SftpIndependentJob : IHostedService
    {
        private static SemaphoreSlim _semaphore;
        private readonly ILogger<SftpIndependentJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly List<Timer> _timers = new List<Timer>();

        private readonly string JobName = nameof(SftpIndependentJob);
        private JobStatus CurrentJobStatus;

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger)
        {
            this._serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                this._logger.LogInformation("Starting SFTP Independent Upload Job...");

                _semaphore = new SemaphoreSlim(1, 1);

                this.GetConfiguration(out SftpConfigModel config, out int prodTimeSpan, out int sbTimeSpan, out JobDisplayManager jobManager);

                if (config.IncludeProduction)
                {
                    Timer timerProd = new Timer(
                        async state => await this.RunFileCheckAndUpload(state, true, config.ProductionFolder, config, jobManager), null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(prodTimeSpan));

                    this._timers.Add(timerProd);
                }

                if (config.IncludeSandbox)
                {
                    Timer timerSB = new Timer(
                        async state => await this.RunFileCheckAndUpload(state, false, config.SandboxFolder, config, jobManager), null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(sbTimeSpan));

                    this._timers.Add(timerSB);
                }

                this._logger.LogInformation("SFTP Independent Upload Job Started Successfully!");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message + " Error starting SFTP independent Upload job");
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("SFTP Independent Upload Job stopped");

            foreach (Timer timer in this._timers) await timer.DisposeAsync();
        }

        private void GetConfiguration(out SftpConfigModel config, out int prodTimeSpan, out int sbTimeSpan, out JobDisplayManager jobManager)
        {
            prodTimeSpan = 10;
            sbTimeSpan = 15;
            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                jobManager = scope.ServiceProvider.GetService<JobDisplayManager>();

                List<Configuration> configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                    .ToList();

                config = new SftpConfigModel
                {
                    Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                    Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                    UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                    Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                    RecurseFolders =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "RecurseFolders")?.Value),
                    IncludeSandbox =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeSandbox")?.Value),
                    IncludeProduction =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value),
                    ProductionFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value,
                    SandboxFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value,
                    KeyFilesPath = configurations.FirstOrDefault(c => c.Key == "KeyFilesPath")?.Value,
                    UseUnicode =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "UseUnicode")?.Value ?? "False")
                };

                prodTimeSpan = Convert.ToInt32(dbContext.Configurations
                    .FirstOrDefault(c => c.Key == "ProductionTimeSpanCheck")?.Value);
                sbTimeSpan = Convert.ToInt32(dbContext.Configurations
                    .FirstOrDefault(c => c.Key == "SandboxTimeSpanCheck")?.Value);
            }
        }

        private async Task RunFileCheckAndUpload(object state, bool isProduction, string productionOrSandboxFolder,
            SftpConfigModel config, JobDisplayManager jobManager)
        {
            bool result;

            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation($"Running file check and upload at {DateTime.Now}!");

                this.CurrentJobStatus = jobManager.GetJobStatus(this.JobName);

                if (this.CurrentJobStatus == null)
                {
                    this.CurrentJobStatus = new JobStatus(this.JobName) { Status = JobState.Running };

                    jobManager.SetJobStatus(this.JobName, this.CurrentJobStatus);
                }

                jobManager.SetJobStatus(nameof(SftpIndependentJob), this.CurrentJobStatus);

                string path = state?.ToString();

                ConnectionInfo connectionInfo = string.IsNullOrEmpty(config.Password?.Trim())
                    ? connectionInfo = new ConnectionInfo(config.Host, config.Port, config.UserName,
                        new NoneAuthenticationMethod(config.UserName))
                    : new ConnectionInfo(config.Host, config.Port, config.UserName,
                        new PasswordAuthenticationMethod(config.UserName, config.Password));

                if (!string.IsNullOrEmpty(config.KeyFilesPath?.Trim()))
                {
                    PrivateKeyFile[] keyFiles = Directory.GetFiles(config.KeyFilesPath).Select(f => new PrivateKeyFile(f)).ToArray();

                    connectionInfo = new ConnectionInfo(config.Host, config.Port, config.UserName,
                        new PrivateKeyAuthenticationMethod(config.UserName, keyFiles));
                }

                if (config.UseUnicode)
                    connectionInfo.Encoding = Encoding.UTF8;

                this.CurrentJobStatus.Status = JobState.Running;
                jobManager.SetJobStatus(nameof(SftpIndependentJob), this.CurrentJobStatus);

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !File.Exists(path))
                {
                    //do check for all folders/files
                    EnumerationOptions options = new EnumerationOptions
                    {
                        MatchCasing = MatchCasing.CaseInsensitive,
                        MatchType = MatchType.Simple,
                        RecurseSubdirectories = true
                    };

                    List<string> files = Directory.GetFiles(productionOrSandboxFolder, "*", options).ToList();

                    result = await this.ProcessFileAndUpload(isProduction, productionOrSandboxFolder, files, connectionInfo);
                }
                else
                {
                    result = await this.ProcessFileAndUpload(isProduction, productionOrSandboxFolder, new List<string> { path }, connectionInfo);
                }

                if (result)
                {
                    this._logger.LogInformation("File check and upload ran successfully!");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message + $" Error running file check and upload");
            }
            finally
            {
                this.CurrentJobStatus.Status = JobState.Completed;
                jobManager.SetJobStatus(nameof(SftpIndependentJob), this.CurrentJobStatus);

                _semaphore.Release();
            }
        }

        private async Task<bool> ProcessFileAndUpload(bool isProduction, string productionOrSandboxFolder, List<string> files,
            ConnectionInfo connectionInfo)
        {
            try
            {
                return await FileHelpers.UploadFilesToSftp(files, isProduction, productionOrSandboxFolder, this._serviceScopeFactory, this._logger, connectionInfo);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message + " Error uploading file");
            }

            return false;
        }
    }
}
