using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PluginBase
{
    /// <summary>
    /// Plug-in projects must ensure final DLL file is named with
    /// ...Plugin.dll at the end
    /// https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
    /// Also remember to do the bit in the .csproj file of the plugin you create
    /// </summary>
    public interface IRunnable : IDisposable
    {
        ILogger<IRunnable> Logger { get; set; }
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        string OutputFolder { get; set; }
        int StartDelay { get; set; }
        bool IsManualRun { get; set; }//does not need startdelay
        Task<bool> Execute(string filePath);
    }
}
