using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using System;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Controllers
{
    /// <summary>
    /// select plugin -> select file(s) -> run job
    /// </summary>
    //[HandleLicense("All")]
    public class TaskController : Controller
    {
        private ILogger<TaskController> _logger;
        private ApplicationDbContext _dbContext;

        public TaskController(ILogger<TaskController> logger, ApplicationDbContext dbContext)
        {
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
        public IActionResult RunPlugin(string plugin, string file)
        {

            return RedirectToAction("Index");
        }
    }
}
