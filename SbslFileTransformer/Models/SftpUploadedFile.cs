using System;

namespace SbslFileTransformer.Models
{
    public class SftpUploadedFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Md5 { get; set; }
        public DateTime UploadedDate { get; set; }
        public long Size { get; set; }
        public bool IsProduction { get; set; }
        public string FilePath { get; set; }
    }
}
