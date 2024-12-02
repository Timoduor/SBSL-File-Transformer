using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;

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

        protected override string JobName { get; set; } = nameof(GLBalanceExtractorJob);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await ExtractGLBalances(), null,
                TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        private async Task ExtractGLBalances()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running GL Balance Extractor Job");

                var prodFolder = string.Empty;
                var sbFolder = string.Empty;
                var Entity = string.Empty;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    CurrentJobStatus = _jobManager.GetJobStatus(JobName);

                    if (CurrentJobStatus == null)
                    {
                        CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Running };

                        _jobManager.SetJobStatus(JobName, CurrentJobStatus);
                    }

                    var configurations = dbContext.Configurations.ToList();

                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                    var isProd =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                          false.ToString());

                    var options = new EnumerationOptions
                    { RecurseSubdirectories = false, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                    var converter = new BalanceFileConverter(_logger, _serviceScopeFactory, Entity);

                    CurrentJobStatus.Status = JobState.Running;
                    _jobManager.SetJobStatus(JobName, CurrentJobStatus);

                    var count = 0;
                    var total = files.Count;

                    foreach (var file in files)
                    {
                        //if in Kenya do not process files for other countries
                        if (Entity == "IMKE" && !file.ToUpper().Contains("IMKE"))
                        {
                            continue;
                        }

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
                _ = _semaphore.Release();
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
                    {
                        _ = await converter.Execute(file, "Mobile banking");
                    }
                    else if (file.ToLower().Contains("br_sus"))
                    {
                        _ = await converter.Execute(file, "Branch Suspense");
                    }
                    else if (file.ToLower().Contains("mg_sus") || file.ToLower().Contains("mg_balances"))
                    {
                        _ = await converter.Execute(file, "Moneygram");
                    }
                    else if (file.ToLower().Contains("wu_sus") || file.ToLower().Contains("wu_balances") || file.ToLower().Contains("westernunion_balance"))
                    {
                        _ = await converter.Execute(file, "Western Union");
                    }
                    else if (file.ToLower().Contains("treasury_sus"))
                    {
                        _ = await converter.Execute(file, "Treasury");
                    }
                    else if (file.ToLower().Contains("ops_sus"))
                    {
                        _ = await converter.Execute(file, "Operations");
                    }
                    else if (file.ToLower().Contains("cre_sus"))
                    {
                        _ = await converter.Execute(file, "Credit");
                    }
                    else if (file.ToLower().Contains("fin_sus"))
                    {
                        _ = await converter.Execute(file, "Finance");
                    }
                    else if (file.ToLower().Contains("clearing_balance"))
                    {
                        _ = await converter.Execute(file, "Clearing");
                    }
                    else if (file.ToLower().Contains("susp_balances"))
                    {
                        _ = await converter.Execute(file, "suspense");
                    }
                    else if (file.ToLower().Contains("agency_balances"))
                    {
                        _ = await converter.Execute(file, "agency");
                    }
                    else if (file.ToLower().Contains("rswitch_balance"))
                    {
                        _ = await converter.Execute(file, "RSwitch");
                    }
                    else if (file.ToLower().Contains("cards_kenya"))
                    {
                        _ = await converter.Execute(file, "Cards Kenya");
                    }
                    else if (file.ToLower().Contains("cards_uganda"))
                    {
                        _ = await converter.Execute(file, "Cards Uganda");
                    }
                    else if (file.ToLower().Contains("mobile_money"))
                    {
                        _ = await converter.Execute(file, "Mobile Banking");
                    }
                    else if (file.ToLower().Contains("mobile_utility"))
                    {
                        _ = await converter.Execute(file, "Mobile Banking");
                    }
                    else if (file.ToLower().Contains("branch_suspense"))
                    {
                        _ = await converter.Execute(file, "Branch Suspense");
                    }
                    else if (file.ToLower().Contains("treasurybills") || file.ToLower().Contains("treasurybonds"))
                    {
                        _ = await converter.Execute(file, "Treasury Bills/Bonds");
                    }
                    else if (file.ToLower().Contains("tresuspense"))
                    {
                        _ = await converter.Execute(file, "Treasury Suspense");
                    }
                    else if (file.ToLower().Contains("trepmoneymarket"))
                    {
                        _ = await converter.Execute(file, "PLACEMENT/BORROWING ACCT");
                    }
                    else if (file.ToLower().Contains("trecontiliab"))
                    {
                        _ = await converter.Execute(file, "ContingentLiability");
                    }
                    else if (file.ToLower().Contains("trecontiasset"))
                    {
                        _ = await converter.Execute(file, "ContingentAsset");
                    }
                    else if (file.ToLower().Contains("treintexp"))
                    {
                        _ = await converter.Execute(file, "GL Entries Interest Expense");
                    }
                    else if (file.ToLower().Contains("treintinc"))
                    {
                        _ = await converter.Execute(file, "GL Entries Interest Income");
                    }
                    else if (file.ToLower().Contains("treposition"))
                    {
                        _ = await converter.Execute(file, "Position Account");
                    }
                    else if (file.ToLower().Contains("pos_pay"))
                    {
                        _ = await converter.Execute(file, "CARDS RWANDA");
                    }
                    else if (file.ToLower().Contains("mobile_banking"))
                    {
                        _ = await converter.Execute(file, "Mobile Banking");
                    }
                    else if (file.ToLower().Contains("spenn_micro"))
                    {
                        _ = await converter.Execute(file, "SPENN");
                    }
                    else if (file.ToLower().Contains("ria_bal"))
                    {
                        _ = await converter.Execute(file, "RIA");
                    }
                    else
                    {
                        _ = await converter.Execute(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
