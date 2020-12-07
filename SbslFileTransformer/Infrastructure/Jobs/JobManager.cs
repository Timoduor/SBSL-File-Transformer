using Microsoft.EntityFrameworkCore;
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
        private ILogger<InputFileWatcher> _fileLogger;

        public JobManager(ILogger<JobManager> logger, ILogger<IRunnable> jobLogger, IServiceScopeFactory serviceScopeFactory, ILogger<InputFileWatcher> fileLogger)
        {
            _fileLogger = fileLogger;
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

                    validJobs = (await dbContext.Plugins.ToListAsync()).Where(v => Directory.Exists(v.InputFolder));
                }

                foreach (var job in validJobs)
                {
                    //maybe use Tasks here

                    var runnable = _jobs.FirstOrDefault(j => j.Id == job.Id);

                    //ensure plugin projects or solution are rebuilt regular so they are picked up by reflection
                    if (runnable != null)
                    {
                        EnsureJobDirectoriesExist(job);

                        runnable.OutputFolder = job.OutputFolder;
                        runnable.Logger = _jobLogger;

                        var fileWatcher = new InputFileWatcher(job.InputFolder, _fileLogger);

                        fileWatcher.ProcessFile = async fileToProcess => await runnable.Execute(fileToProcess);

                        _inputFileWatcher.Add(fileWatcher);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private static void EnsureJobDirectoriesExist(Plugin job)
        {
            if (string.IsNullOrEmpty(job.InputFolder))
            {
                job.InputFolder = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), $@"SBSLETL\{job.Name.Replace(" ", "")}\input");

            }
            Directory.CreateDirectory(job.InputFolder);

            if (string.IsNullOrEmpty(job.OutputFolder))
            {
                job.OutputFolder = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), $@"SBSLETL\{job.Name.Replace(" ", "")}\output");
            }
            Directory.CreateDirectory(job.OutputFolder);
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

            _logger.LogInformation("Job Manager stopped");
        }
    }
}
