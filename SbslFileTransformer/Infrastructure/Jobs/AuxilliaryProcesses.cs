using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class AuxilliaryProcesses : IHostedService
    {
        ILogger<AuxilliaryProcesses> _logger;
        Timer _timer;
        public AuxilliaryProcesses(ILogger<AuxilliaryProcesses> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer((state) => RestartService(), null, TimeSpan.Zero,
                                                            TimeSpan.FromHours(2));

            return Task.CompletedTask;
        }

        private void RestartService()
        {
            if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 4)
            {
                StaticHelpers.RestartService("SBSL ETL Service");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auxilliary Services stopped!");
            return Task.CompletedTask;
        }
    }
}
