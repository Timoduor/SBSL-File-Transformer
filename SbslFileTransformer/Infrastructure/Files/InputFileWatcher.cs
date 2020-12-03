using SbslFileTransformer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Files
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public sealed class InputFileWatcher : IDisposable
    {
        private FileSystemWatcher _fileWatcher;

        public Func<string, Task<bool>> ProcessFile;

        public InputFileWatcher(string inputFolder)
        {
            _fileWatcher = new FileSystemWatcher
            {
                Path = inputFolder,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
            };


            _fileWatcher.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            _fileWatcher.Created += OnCreated;

            //_fileWatcher.Changed += OnChanged; //MIGHT NEED THESE LATER ON
            //_fileWatcher.Deleted += OnDeleted;
            //_fileWatcher.Renamed += OnRenamed;
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await ProcessFile(e.FullPath);
        }

        public void Dispose()
        {
            _fileWatcher.Dispose();
        }
    }
}
