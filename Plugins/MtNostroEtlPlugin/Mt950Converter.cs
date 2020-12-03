using Microsoft.Extensions.Logging;
using PluginBase;
using System;
using System.Threading.Tasks;

namespace MtNostroEtlPlugin
{
    public class Mt950Converter : IRunnable
    {
        public ILogger<IRunnable> Logger { get; set; }

        public Guid Id => new Guid("ab25e115-1be3-48a3-923b-30ddb2b5c366");

        public string Name => "MT 940/950 Converter";

        public string Description => "This plugin converts the Tz MT files into Standard Ke MT files";

        public string OutputFolder { get; set; }
        public int StartDelay { get; set; }
        public bool IsManualRun { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Execute(string filePath)
        {
            Logger.LogDebug("IT IS REACHED!!");

            return true;
        }
    }
}
