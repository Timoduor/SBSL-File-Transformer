using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using System;
using System.IO;
using System.Linq;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Controllers
{
    /// <summary>
    ///     select plugin -> select file(s) -> run job
    /// </summary>
    //[HandleLicense("All")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ILogger<TaskController> logger, ApplicationDbContext dbContext)
        {
            this._logger = logger;
            this._dbContext = dbContext;
        }

        public IActionResult Index(Guid? pluginId)
        {
            System.Collections.Generic.List<Plugin> plugins = this._dbContext.Plugins.ToList();

            Plugin selectedPlugin = null;

            if (pluginId != null)
                selectedPlugin = plugins.FirstOrDefault(p => p.Id == pluginId);
            else
                selectedPlugin = plugins.Last();

            System.Collections.Generic.List<string> files = Directory.GetFiles(selectedPlugin.InputFolder).ToList();

            TaskViewModel taskVM = new TaskViewModel();
            taskVM.Files.AddRange(files.Select(f => new FileInfo(f)));

            foreach (Plugin plugin in plugins)
            {
                if (string.IsNullOrEmpty(plugin.InputFolder))
                    continue;

                if (!Directory.Exists(plugin.InputFolder))
                    Directory.CreateDirectory(plugin.InputFolder);

                taskVM.Plugins.Add(new PluginViewModel
                {
                    Id = plugin.Id,
                    InputFolder = plugin.InputFolder,
                    IsSelected = plugin.Id == selectedPlugin.Id,
                    Name = plugin.Name
                });
            }

            return this.View(taskVM);
        }

        public IActionResult Jobs()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult RunPlugin(string plugin, string file)
        {
            return this.RedirectToAction("Index");
        }
    }
}