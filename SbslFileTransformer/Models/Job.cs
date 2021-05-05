using SbslFileTransformer.Infrastructure.Jobs;

namespace SbslFileTransformer.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string JobDescription { get; set; }
        //comma separated values
        public string RequiredPaths { get; set; }
        //comma separated values
        public string OptionalPaths { get; set; }
        //comma separated values
        public string FileExtensions { get; set; }
    }
}
