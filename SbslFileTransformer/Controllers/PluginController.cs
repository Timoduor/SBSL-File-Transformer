using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PluginBase;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Plugins;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    [AllowAnonymous]
    public class PluginController : Controller
    {
        private ILogger<PluginManager> _pluginLogger;
        private ApplicationDbContext _dbContext;
        private PluginManager _pluginManager;

        public PluginController(ILogger<PluginManager> pluginLogger, ApplicationDbContext dbContext, PluginManager pluginManager)
        {
            _pluginLogger = pluginLogger;
            _dbContext = dbContext;
            _pluginManager = pluginManager;
        }

        public async Task<IActionResult> Index()
        {
            var plugins = _pluginManager.GetPlugins();

            var savedPlugins = _dbContext.Plugins;

            var unsaved = plugins.Where(p => !savedPlugins.Select(s => s.Id).Contains(p.Id));

            await SaveNewPlugins(unsaved);

            return View(savedPlugins);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            //only allow editing of input/output folders check interval, check time
            var plugin = await _dbContext.Plugins.FindAsync(id);

            return View(plugin);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Plugin plugin)
        {
            var pluginToEdit = await _dbContext.Plugins.FindAsync(plugin.Id);

            pluginToEdit.StartDelay = plugin.StartDelay;
            pluginToEdit.InputFolder = plugin.InputFolder;
            pluginToEdit.OutputFolder = plugin.OutputFolder;

            _dbContext.Entry(pluginToEdit).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private async Task SaveNewPlugins(IEnumerable<IRunnable> newPlugins)
        {
            foreach(var plugin in newPlugins)
            {
                var p = new Plugin
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Description = plugin.Description,
                    OutputFolder = plugin.OutputFolder
                };

                _dbContext.Plugins.Add(p);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
