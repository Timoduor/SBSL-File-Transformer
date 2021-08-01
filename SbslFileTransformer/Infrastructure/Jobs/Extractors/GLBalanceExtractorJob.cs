using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Extractors
{
    public class GLBalanceExtractorJob : ConverterJobBase<GLBalanceExtractorJob>, IHostedService
    {
        public GLBalanceExtractorJob(IServiceScopeFactory serviceScopeFactory, ILogger<GLBalanceExtractorJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await ExtractGLBalances(), null,
                TimeSpan.FromSeconds(new Random().Next(10, 30)), TimeSpan.FromMinutes(10));

            _semaphore = new SemaphoreSlim(1, 1);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
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

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                        .ToList();

                    Entity = dbContext.Configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;

                    var isProd =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value ??
                                          false.ToString());

                    var options = new EnumerationOptions
                    { RecurseSubdirectories = false, MatchCasing = MatchCasing.CaseInsensitive };

                    var files = Directory.GetFiles(prodFolder, "*.csv", options).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.csv", options));

                    var converter = new BalanceFileConverter(_logger, _serviceScopeFactory, Entity);

                    foreach (var file in files)
                    {
                        if (Entity == "IMKE" && !file.ToUpper().Contains("IMKE")) continue;

                        if ((file.ToLower().Contains("_balance".ToLower()) ||
                             file.ToLower().Contains("_bal".ToLower())) && Path.GetExtension(file.ToLower()) != ".txt")
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

                                else if (file.ToLower().Contains("treasurybills") || file.ToLower().Contains("treasurybonds"))
                                    await converter.Execute(file, "Treasury Bills/Bonds");

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