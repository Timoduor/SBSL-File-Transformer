using System.ComponentModel.DataAnnotations;

namespace SbslFileTransformer.Models
{
    public class SftpConfigModel
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RecurseFolders { get; set; }
        public bool IncludeSandbox { get; set; }
        public bool IncludeProduction { get; set; }
        public string SandboxFolder { get; set; }
        public string ProductionFolder { get; set; }
        public string Entity { get; set; }
    }
}
