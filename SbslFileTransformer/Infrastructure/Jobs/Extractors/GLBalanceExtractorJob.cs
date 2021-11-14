using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Extractors
{
    public class GLBalanceExtractorJob : ConverterJobBase<GLBalanceExtractorJob>, IHostedService
    {
        public GLBalanceExtractorJob(IServiceScopeFactory serviceScopeFactory, ILogger<GLBalanceExtractorJob> logger, JobDisplayManager jobManager)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _jobManager = jobManager;
        }

        protected override string JobName { get; set; } = nameof(ImsBalanceExtractorJob);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await ExtractGLBalances(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        private async Task ExtractGLBalances()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running GL Balance Extractor Job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    CurrentJobStatus = _jobManager.GetJobStatus(JobName);

                    if (CurrentJobStatus == null)
                    {
                        CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Running };

                        _jobManager.SetJobStatus(JobName, CurrentJobStatus);
                    }

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                    bool isProd =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                          false.ToString());

                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = false, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                    BalanceFileConverter converter = new BalanceFileConverter(_logger, _serviceScopeFactory, Entity);

                    CurrentJobStatus.Status = JobState.Running;
                    _jobManager.SetJobStatus(JobName, CurrentJobStatus);

                    int count = 0;
                    int total = files.Count;

                    foreach (string file in files)
                    {
                        //if in Kenya do not process files for other countries
                        if (Entity == "IMKE" && !file.ToUpper().Contains("IMKE"))
                            continue;

                        await ProcessFile(file, converter);

                        count++;

                        CurrentJobStatus.ProgressMessage = $"Currently processing {file}... {count} of {total}";
                        CurrentJobStatus.SetProgress(count, total);
                        _jobManager.SetJobStatus(JobName, CurrentJobStatus);
                    }

                    CurrentJobStatus.Status = JobState.Completed;
                    _jobManager.SetJobStatus(JobName, CurrentJobStatus);
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

        private async Task ProcessFile(string file, BalanceFileConverter converter)
        {
            if ((file.ToLower().Contains("_balance".ToLower()) ||
                 file.ToLower().Contains("_bal".ToLower())) && Path.GetExtension(file.ToLower()) != ".txt")
            {
                try
                {
                    if (
                        file.ToLower().Contains("util_balance".ToLower()) ||
                        file.ToLower().Contains("mb_balance".ToLower())
                        || file.ToLower().Contains("selcom_balance".ToLower()) ||
                        file.ToLower().Contains("selcomdisb_balance") ||
                        file.ToLower().Contains("float_balance".ToLower())
                        || file.ToLower().Contains("b2w_balance".ToLower()) ||
                        file.ToLower().Contains("w2b_balance".ToLower()))
                        await converter.Execute(file, "Mobile banking");

                    else if (file.ToLower().Contains("br_sus"))
                        await converter.Execute(file, "Branch Suspense");

                    else if (file.ToLower().Contains("mg_sus"))
                        await converter.Execute(file, "Moneygram");

                    else if (file.ToLower().Contains("wu_sus") || file.ToLower().Contains("westernunion_balance"))
                        await converter.Execute(file, "Western Union");

                    else if (file.ToLower().Contains("treasury_sus"))
                        await converter.Execute(file, "Treasury");

                    else if (file.ToLower().Contains("ops_sus"))
                        await converter.Execute(file, "Operations");

                    else if (file.ToLower().Contains("cre_sus"))
                        await converter.Execute(file, "Credit");

                    else if (file.ToLower().Contains("fin_sus"))
                        await converter.Execute(file, "Finance");

                    else if (file.ToLower().Contains("clearing_balance"))
                        await converter.Execute(file, "Clearing");

                    else if (file.ToLower().Contains("rswitch_balance"))
                        await converter.Execute(file, "RSwitch");

                    else if (file.ToLower().Contains("cards_kenya"))
                        await converter.Execute(file, "Cards Kenya");

                    else if (file.ToLower().Contains("mobile_money"))
                        await converter.Execute(file, "Mobile Banking");

                    else if (file.ToLower().Contains("mobile_utility"))
                        await converter.Execute(file, "Mobile Banking");

                    else if (file.ToLower().Contains("branch_suspense"))
                        await converter.Execute(file, "Branch Suspense");

                    else if (file.ToLower().Contains("treasurybills") || file.ToLower().Contains("treasurybonds"))
                        await converter.Execute(file, "Treasury Bills/Bonds");

                    else if (file.ToLower().Contains("tresuspense"))
                        await converter.Execute(file, "Treasury Suspense");

                    else if (file.ToLower().Contains("trepmoneymarket"))
                        await converter.Execute(file, "PLACEMENT/BORROWING ACCT");

                    else if (file.ToLower().Contains("trecontiliab"))
                        await converter.Execute(file, "ContingentLiability");

                    else if (file.ToLower().Contains("trecontiasset"))
                        await converter.Execute(file, "ContingentAsset");

                    else if (file.ToLower().Contains("treintexp"))
                        await converter.Execute(file, "GL Entries Interest Expense");

                    else if (file.ToLower().Contains("treintinc"))
                        await converter.Execute(file, "GL Entries Interest Income");

                    else if (file.ToLower().Contains("treposition"))
                        await converter.Execute(file, "Position Account");

                    else
                        await converter.Execute(file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}