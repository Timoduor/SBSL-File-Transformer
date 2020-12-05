using System.Collections.Generic;

namespace SbslFileTransformer.Models
{
    public class TaskViewModel
    {

        //plugin id plus file paths
        public List<Plugin> Plugins{ get; set; } = new List<Plugin>();
        public List<string> Files { get; set; } = new List<string>();
    }
}
