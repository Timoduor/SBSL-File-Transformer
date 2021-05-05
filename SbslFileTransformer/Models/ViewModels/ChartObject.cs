using System.Collections.Generic;

namespace SbslFileTransformer.Models.ViewModels
{
    public class ChartObjects
    {
        public Dictionary<string, int> Logs { get; set; }

        public Dictionary<string, int> Reports { get; set; }

        public Dictionary<string, int> UploadedFiles { get; set; }
    }
}
