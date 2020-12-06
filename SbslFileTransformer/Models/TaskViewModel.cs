using System.Collections.Generic;
using System.IO;

namespace SbslFileTransformer.Models
{
    public class TaskViewModel
    {

        //plugin id plus file paths
        public List<Plugin> Plugins{ get; set; } = new List<Plugin>();
        public List<FileInfo> Files { get; set; } = new List<FileInfo>();
    }
}
