using System.Collections.Generic;

namespace SbslFileTransformer.Models.ViewModels
{
    public class ChartObjects
    {
        public Dictionary<string, int> Logs { get; set; }

        public Dictionary<string, int> Reports { get; set; }

        public Dictionary<string, int> UploadedFilesPerDay { get; set; }
        public Dictionary<string, int> UploadedFilesPerWeek { get; set; }
        public Dictionary<string, int> UploadedFilesPerMonth { get; set; }
    }
}