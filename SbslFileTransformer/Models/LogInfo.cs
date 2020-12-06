using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Models
{
    public class LogInfo
    {
        public IOrderedEnumerable<IFileInfo> FileInfos { get; set; }
        public IOrderedEnumerable<SqliteLog> SqliteLogs { get; set; }
    }

    public class SqliteLog
    {
        public int Id { get; set; }
        public string TimeStamp { get; set; }
        public string Level { get; set; }
        public string Exception { get; set; }
        public string RenderedMessage { get; set; }
        public string Properties { get; set; }
    }
}
