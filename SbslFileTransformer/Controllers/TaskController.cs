using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Plugins;
using SbslFileTransformer.Models;
using System;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Controllers
{
    /// <summary>
    /// select plugin -> select file(s) -> run job
    /// </summary>
    public class TaskController : Controller
    {
        private PluginManager _pluginManager;
        private ILogger<TaskController> _logger;
        private ApplicationDbContext _dbContext;

        public TaskController(PluginManager pluginManager, ILogger<TaskController> logger, ApplicationDbContext dbContext)
        {
            _pluginManager = pluginManager;
            _logger = logger;
            _dbContext = dbContext;
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
            taskVM.Files.AddRange(files);

            foreach (var plugin in plugins)
            {
                if (string.IsNullOrEmpty(plugin.InputFolder) || !Directory.Exists(plugin.InputFolder))
                    continue;

                taskVM.Plugins.Add(plugin);
            }

            return View(taskVM);
        }

        public IActionResult RunPlugin(TaskViewModel tvm)
        {
            //open the output directory when done

            return RedirectToAction("Index");
        }
    }
}
