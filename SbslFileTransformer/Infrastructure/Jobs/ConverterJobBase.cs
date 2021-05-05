using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using System.Collections.Generic;
using System.Threading;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public abstract class ConverterJobBase<T>
    {
        protected ILogger<T> _logger;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected static SemaphoreSlim _semaphore;
        protected Timer _timer;
        protected EmailSender _emailSender;

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
