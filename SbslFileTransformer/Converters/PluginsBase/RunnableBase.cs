using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PluginBase
{
    public abstract class RunnableBase : IRunnable
    {
        public abstract ILogger Logger { get; set; }

        public abstract Guid Id { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string OutputFolder { get; set; }
        public abstract int StartDelay { get; set; }
        public abstract bool IsManualRun { get; set; }
        public abstract string Entity { get; set; }

        public abstract void Dispose();

        public async virtual Task<bool> Execute(string filePath)
        {
            await Task.Delay(TimeSpan.FromSeconds(StartDelay));

            return true;
        }
    }
}
