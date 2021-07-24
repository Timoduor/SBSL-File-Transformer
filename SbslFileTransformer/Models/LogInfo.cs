using Microsoft.Extensions.FileProviders;
using System.Linq;

namespace SbslFileTransformer.Models
{
    public class LogInfo
    {
        public IOrderedEnumerable<IFileInfo> FileInfos { get; set; }
        public IOrderedEnumerable<SqliteLog> SqliteLogs { get; set; }
        public IOrderedEnumerable<SftpUploadedFile> UploadedFiles { get; set; }
    }
}