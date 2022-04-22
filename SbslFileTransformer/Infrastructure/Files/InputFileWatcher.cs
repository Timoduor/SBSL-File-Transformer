using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Files
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public sealed class InputFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _fileWatcher;

        private readonly ILogger _logger;

        public Func<string, Task> ProcessFile;

        public InputFileWatcher(string inputFolder, ILogger logger)
        {
            this._logger = logger;

            this._fileWatcher = new FileSystemWatcher
            {
                Path = inputFolder,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };


            this._fileWatcher.NotifyFilter = NotifyFilters.FileName
                                             | NotifyFilters.DirectoryName;

            this._fileWatcher.Created += this.OnCreated;

            this._fileWatcher.Changed += this.OnChanged;
            //_fileWatcher.Deleted += OnDeleted;//MIGHT NEED THESE LATER ON
            //_fileWatcher.Renamed += OnRenamed;

            this._fileWatcher.Error += this._fileWatcher_Error;
        }

        public void Dispose()
        {
            this._fileWatcher.Dispose();
        }

        private void _fileWatcher_Error(object sender, ErrorEventArgs e)
        {
            this._logger.LogError(e.GetException(), e.GetException().Message);
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(20 * 1000); //20 second delay before file starts uploading

            await this.ProcessFile(e.FullPath);

            this._logger.LogInformation($"File {e.FullPath} changed!");
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(20 * 1000); //20 second delay before file starts uploading

            await this.ProcessFile(e.FullPath);

            this._logger.LogInformation($"File {e.FullPath} created!");
        }
    }
}