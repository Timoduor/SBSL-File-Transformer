using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Infrastructure.Plugins;
using System.IO;

namespace SbslFileTransformer.Controllers
{
    public class PluginController : Controller
    {
        private ILogger<PluginManager> _pluginLogger;

        public PluginController(ILogger<PluginManager> pluginLogger)
        {
            _pluginLogger = pluginLogger;
        }

        public IActionResult Index()
        {
            var pluginManager = new PluginManager(_pluginLogger);

            var plugins = pluginManager.GetPlugins(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"));

            return View(plugins);
        }
    }
}
