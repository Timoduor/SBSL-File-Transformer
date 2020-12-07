using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PluginBase;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;
using SbslFileTransformer.Infrastructure.Plugins;
using SbslFileTransformer.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    /// <summary>
    /// select plugin -> select file(s) -> run job
    /// </summary>
    [HandleLicense("All")]
    public class TaskController : Controller
    {
        private PluginManager _pluginManager;
        private ILogger<TaskController> _logger;
        private ILogger<IRunnable> _pluginLogger;
        private ApplicationDbContext _dbContext;

        public TaskController(PluginManager pluginManager, ILogger<TaskController> logger,
                            ApplicationDbContext dbContext, ILogger<IRunnable> pluginLogger)
        {
            _pluginManager = pluginManager;
            _logger = logger;
            _dbContext = dbContext;
            _pluginLogger = pluginLogger;
        }

        public IActionResult Index(Guid? pluginId)
        {
            var plugins = _dbContext.Plugins.ToList();

            Plugin selectedPlugin = null;

            if (pluginId != null)
            {
                selectedPlugin = plugins.FirstOrDefault(p => p.Id == pluginId);
            }
            else
            {
                selectedPlugin = plugins.Last();
            }

            var files = Directory.GetFiles(selectedPlugin.InputFolder).ToList();

            var taskVM = new TaskViewModel();
            taskVM.Files.AddRange(files.Select(f => new FileInfo(f)));

            foreach (var plugin in plugins)
            {
                if (string.IsNullOrEmpty(plugin.InputFolder))
                    continue;

                if (!Directory.Exists(plugin.InputFolder))
                    Directory.CreateDirectory(plugin.InputFolder);

                taskVM.Plugins.Add(new PluginViewModel {
                    Id = plugin.Id,
                    InputFolder = plugin.InputFolder,
                    IsSelected = plugin.Id == selectedPlugin.Id,
                    Name = plugin.Name
                });
            }

            return View(taskVM);
        }

        [HttpPost]
        public async Task<IActionResult> RunPlugin(string plugin, string file)
        {
            //open the output directory when done
            var pluginToRun = _pluginManager.GetPlugins().FirstOrDefault(p => p.Id == new Guid(plugin));

            if(pluginToRun != null)
            {
                pluginToRun.IsManualRun = true;
                pluginToRun.Logger = _pluginLogger;

                var success = await pluginToRun.Execute(file);

                if (success)
                {
                    TempData["Message"] = "Converter ran successfully to completion";
                }
                else
                {
                    TempData["Message"] = "Converter failed!";
                }
            }

            return RedirectToAction("Index");
        }
    }
}
