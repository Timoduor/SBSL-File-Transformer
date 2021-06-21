using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace SbslFileTransformer.Models
{
    public class LogInfo
    {
        public IOrderedEnumerable<IFileInfo> FileInfos { get; set; }
        public IOrderedEnumerable<SqliteLog> SqliteLogs { get; set; }
        public IOrderedEnumerable<SftpUploadedFile> UploadedFiles { get; set; }
    }
}