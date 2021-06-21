using System;

namespace SbslFileTransformer.Models
{
    public class PluginViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string InputFolder { get; set; }
        public bool IsSelected { get; set; }
    }
}