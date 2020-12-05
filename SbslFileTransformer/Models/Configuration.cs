using SbslFileTransformer.Models.Enums;
using System;

namespace SbslFileTransformer.Models
{
    public class Configuration
    {
        public int Id { get; set; }
        public ConfigurationType ConfigType { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime Updated { get; set; }
    }
}
