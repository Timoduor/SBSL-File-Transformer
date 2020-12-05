using System;

namespace SbslFileTransformer.Models
{
    public class Plugin
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }
        public int StartDelay { get; set; }
    }
}
