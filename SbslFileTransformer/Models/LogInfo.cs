using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace SbslFileTransformer.Models
{
    public class LogInfo
    {
        public IOrderedEnumerable<IFileInfo> FileInfos { get; set; }
        public IOrderedEnumerable<LogEntries> SqliteLogs { get; set; }
        public IOrderedEnumerable<SftpUploadedFile> UploadedFiles { get; set; }
    }
}