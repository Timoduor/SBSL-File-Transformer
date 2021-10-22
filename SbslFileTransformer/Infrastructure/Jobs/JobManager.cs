using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class JobManager
    {
        private readonly IMemoryCache _memoryCache;

        public JobManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public List<string> GetJobNames()
        {
            IEnumerable<Type> jobs = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(type => typeof(IHostedService).IsAssignableFrom(type) && !type.IsInterface);

            List<string> jobNames = new List<string>();

            foreach (var job in jobs)
            {
                jobNames.Add(job.Name);
            }

            return jobNames;
        }

        public void SetJobStatus(string jobName, JobStatus status)
        {
            _memoryCache.Set(jobName, status);
        }

        /// <summary>
        /// Returns null if job status is not found in MemoryCache
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public JobStatus GetJobStatus(string jobName)
        {
            if (_memoryCache.TryGetValue(jobName, out JobStatus status))
            {
                return status;
            }

            return null;
        }

        /// <summary>
        /// List of jobs and their statuses
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, JobStatus> GetJobStatuses()
        {
            var jobs = GetJobNames();
            var jobStatuses = new Dictionary<string, JobStatus>();
            
            foreach(var job in jobs)
            {
                jobStatuses.Add(job, GetJobStatus(job));
            }

            return jobStatuses;
        }
    }

    public class JobStatus
    {
        public JobStatus(string jobName)
        {
            JobName = jobName;
        }

        public JobState Status { get; set; } = JobState.Completed;
        public string JobName {  get; set; }
        public int PercentageProgress { get; set; }
        public string ProgressMessage {  get; set; }

        public void SetProgress(int currentCount, int total)
        {
            PercentageProgress = (currentCount * 100) / total;
        }
    }
}
