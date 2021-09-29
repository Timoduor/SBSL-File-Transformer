using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Infrastructure.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class VisionRecordExtractorJob : ConverterJobBase<VisionRecordExtractorJob>, IHostedService
    {
        public VisionRecordExtractorJob(ILogger<VisionRecordExtractorJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Vision Record Extractor Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await VisionRecordExtractor(), null,
                TimeSpan.FromSeconds(new Random().Next(15, 60)), TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async Task VisionRecordExtractor()
        {
            throw new NotImplementedException();
        }
    }
}
