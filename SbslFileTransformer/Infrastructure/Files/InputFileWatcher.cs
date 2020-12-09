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
        private FileSystemWatcher _fileWatcher;

        public Func<string, Task> ProcessFile;

        private ILogger<InputFileWatcher> _logger;
        public InputFileWatcher(string inputFolder, ILogger<InputFileWatcher> logger)
        {
            _logger = logger;

            _fileWatcher = new FileSystemWatcher
            {
                Path = inputFolder,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
            };


            _fileWatcher.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            _fileWatcher.Created += OnCreated;

            _fileWatcher.Changed += OnChanged;
            //_fileWatcher.Deleted += OnDeleted;//MIGHT NEED THESE LATER ON
            //_fileWatcher.Renamed += OnRenamed;

            _fileWatcher.Error += _fileWatcher_Error;
        }

        private void _fileWatcher_Error(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), e.GetException().Message);
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(7 * 1000);

            await ProcessFile(e.FullPath);

            _logger.LogInformation($"File {e.FullPath} changed!");
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(7 * 1000);

            await ProcessFile(e.FullPath);

            _logger.LogInformation($"File {e.FullPath} created!");
        }

        public void Dispose()
        {
            _fileWatcher.Dispose();
        }
    }
}
