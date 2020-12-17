using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SbslFileTransformer.Infrastructure.Plugins
{
    public class PluginManager
    {
        private ILogger<PluginManager> _logger;
        private ILogger<IRunnable> _loggerRunnable;

        public PluginManager(ILogger<PluginManager> logger, ILogger<IRunnable> loggerRunnable)
        {
            _logger = logger;
            _loggerRunnable = loggerRunnable;
        }

        public IEnumerable<IRunnable> GetPlugins(string pluginsFolder = "")
        {
            if (string.IsNullOrEmpty(pluginsFolder))
            {
                pluginsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Plugins"); //MIGHT NEED TO HAVE THIS IN A CONFIGURATION INSTEAD
            }

            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false,
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple
            };

            var pluginPaths = Directory.GetFiles(pluginsFolder, "*Plugin.dll", options);

            IEnumerable<IRunnable> commands = pluginPaths
                .Where(p => Path.GetFileName(p).ToLower().EndsWith("plugin.dll"))
                .SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                return CreateCommands(pluginAssembly);
            }).ToList();


            return commands;
        }

        private Assembly LoadPlugin(string pluginLocation)
        {
            _logger.LogDebug($"Loading runnable from: {pluginLocation}");

            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);

            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        private IEnumerable<IRunnable> CreateCommands(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                var fullName = typeof(IRunnable).Module.FullyQualifiedName;

                var fullName2 = type.Module.FullyQualifiedName;

                if (typeof(IRunnable).IsAssignableFrom(type))
                {
                    IRunnable result = Activator.CreateInstance(type) as IRunnable;

                    result.Logger = _loggerRunnable;

                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));

                _logger.LogWarning(
                    $"Can't find any type which implements IRunnable in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }

    }
}
