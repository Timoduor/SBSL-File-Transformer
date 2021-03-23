using SbslFileTransformer.Data;
using System.Collections.Generic;
using System.Linq;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public abstract class ConverterJobBase
    {


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
