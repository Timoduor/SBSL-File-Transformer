using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public abstract class ConverterJobBase<T>
    {
        protected static SemaphoreSlim _semaphore;
        protected EmailSender _emailSender;
        protected ILogger<T> _logger;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected Timer _timer;

        public virtual List<string> ReqPaths { get; set; }
        public virtual List<string> OptPaths { get; set; }
        public virtual List<string> FileExts { get; set; }

        public virtual void LoadContents(ApplicationDbContext dbContext, int jobId)
        {
            //var job = dbContext.Jobs.FirstOrDefault(j => j.Id == jobId);

            //if (job != null)
            //{
            //    ReqPaths = job.RequiredPaths.Split(',').ToList();
            //    FileExts = job.FileExtensions.Split(',').ToList();
            //    OptPaths = job.OptionalPaths.Split(',').ToList();
            //}
        }
    }
}