using System;

namespace PluginBase
{
    /// <summary>
    /// Plug-in projects must ensure final DLL file is named with
    /// ...Plugin.dll at the end
    /// https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
    /// Also remember to do the bit in the .csproj file of the plugin you create
    /// </summary>
    public interface IRunnable
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        string OriginPath { get; set; }
        string DestinationPath { get; set; }
        bool Execute();
    }
}
