using Microsoft.Extensions.Logging;
using PluginBase;
using System;
using System.Threading.Tasks;

namespace MtNostroEtlPlugin
{
    public class Mt950Converter : RunnableBase
    {
        public override ILogger<IRunnable> Logger { get; set; }

        public override Guid Id => new Guid("ab25e115-1be3-48a3-923b-30ddb2b5c366");

        public override string Name => "MT 940/950 Converter";

        public override string Description => "This plugin converts the Tz MT files into Standard Ke MT files";

        public override string OutputFolder { get; set; }
        public override int StartDelay { get; set; }
        public override bool IsManualRun { get; set; }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> Execute(string filePath)
        {
            await base.Execute(filePath);

            Logger.LogDebug("IT IS REACHED!!");

            return true;
        }
    }
}
