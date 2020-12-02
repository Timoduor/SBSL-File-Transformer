using PluginBase;
using System;

namespace CdmEtlPlugin
{
    public class CdmConverter : IRunnable
    {
        public Guid Id => new Guid("abbae997-0ae8-4ce7-9a14-a7b2d84b21db");

        public string Name => "Cdm Converter Plugin";

        public string Description => "This plugin loads cdm excel files and converts them to a csv format that blackline can process";

        public string OriginPath { get; set; }
        public string DestinationPath { get; set; }

        public bool Execute()
        {
            Console.WriteLine("It works");

            return true;
        }
    }
}
