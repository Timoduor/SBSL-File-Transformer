using Microsoft.Extensions.Logging;
using PluginBase;
using System;
using System.Threading.Tasks;

namespace CdmEtlPlugin
{
    public sealed class CdmConverter : RunnableBase
    {
        public override Guid Id => new Guid("abbae997-0ae8-4ce7-9a14-a7b2d84b21db");

        public override string Name => "Cdm Converter";

        public override string Description => "This plugin loads cdm excel files and converts them to a csv format that blackline can process";
        public override string OutputFolder { get; set; }
        public override ILogger<IRunnable> Logger { get; set; }
        public override int StartDelay { get; set; }
        public override bool IsManualRun { get; set; }
        public override string Entity { get; set; }

        public override void Dispose()
        {
            //dispose any resources here
        }

        public async Task<bool> Execute(string filePath)
        {
            //await whatever logic in a task so that it runs in a separate thread
            try
            {
                //check if file is valid
                //process it
                Console.WriteLine($"Job with file {filePath} started successfully!");
                Logger.LogDebug("Plugin is HIT!!!!!");

                return true; //for success false otherwise
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message + typeof(CdmConverter).FullName);

                return false;
            }

        }
    }
}
