using System;
using System.ComponentModel.DataAnnotations;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Models
{
    public class Configuration
    {
        public int Id { get; set; }

        [Required] public ConfigurationType ConfigType { get; set; }

        [Required] public string Key { get; set; }

        [Required] public string Value { get; set; }

        public DateTime Updated { get; set; } = DateTime.Now;
    }
}