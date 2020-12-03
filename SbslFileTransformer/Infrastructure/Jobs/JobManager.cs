using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PluginBase;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Files;
using SbslFileTransformer.Infrastructure.Plugins;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public sealed class JobManager : IHostedService
    {
        private List<InputFileWatcher> _inputFileWatcher = new List<InputFileWatcher>();
        private List<IRunnable> _jobs = new List<IRunnable>();
        private IServiceScopeFactory _serviceScopeFactory;

        private ILogger<JobManager> _logger;
        private ILogger<IRunnable> _jobLogger;

        public JobManager(ILogger<JobManager> logger, ILogger<PluginManager> pluginLogger, ILogger<IRunnable> jobLogger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _jobLogger = jobLogger;
            _serviceScopeFactory = serviceScopeFactory;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _jobs = scope.ServiceProvider.GetService<PluginManager>().GetPlugins().ToList();
            }
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //Get the valid jobs and assign each a filewatcher that will execute it when a new file is created

            try
            {
                IEnumerable<Plugin> validJobs;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    validJobs = dbContext.Plugins.ToList().Where(v => Directory.Exists(v.InputFolder));
                }

                foreach (var job in validJobs)
                {
                    //maybe use Tasks here

                    var runnable = _jobs.FirstOrDefault(j => j.Id == job.Id);

                    runnable.OutputFolder = job.OutputFolder;
                    runnable.Logger = _jobLogger;

                    var fileWatcher = new InputFileWatcher(job.InputFolder);

                    fileWatcher.ProcessFile = async fileToProcess => await runnable.Execute(fileToProcess);

                    _inputFileWatcher.Add(fileWatcher);
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var watcher in _inputFileWatcher)
            {
                watcher.Dispose();
            }

            foreach (var job in _jobs)
            {
                job.Dispose();
            }
        }
    }
}
