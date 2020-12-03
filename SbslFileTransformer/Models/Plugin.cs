using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Models
{
    public class Plugin
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }
        public int CheckInterval { get; set; }
        public DateTime CheckTime { get; set; }
    }
}
