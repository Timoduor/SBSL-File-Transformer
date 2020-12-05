namespace SbslFileTransformer.Models
{
    public class SftpConfigModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RecurseFolders { get; set; }
        public bool IncludeSandbox { get; set; }
        public bool IncludeProduction { get; set; }
        public string SandboxFolder { get; set; }
        public string ProductionFolder { get; set; }
    }
}
