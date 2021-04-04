using System.Collections.Generic;
using System.IO;

namespace SbslFileTransformer.Models
{
    public class TaskViewModel
    {

        //plugin id plus file paths
        public List<PluginViewModel> Plugins{ get; set; } = new List<PluginViewModel>();
        public List<FileInfo> Files { get; set; } = new List<FileInfo>();
    }
}
