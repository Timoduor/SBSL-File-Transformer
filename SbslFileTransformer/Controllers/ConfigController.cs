using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    public class ConfigController : Controller
    {
        private ApplicationDbContext _dbContext;

        public ConfigController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var configs = await _dbContext.Configurations.ToListAsync();

            return View(configs);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Sftp()
        {
            var configurations = await _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToListAsync();

            if (configurations.Count == 9)
            {

                var config = new SftpConfigModel
                {
                    Host = configurations.First(c => c.Key == "Host").Value,
                    Port = Convert.ToInt32(configurations.First(c => c.Key == "Port").Value),
                    UserName = configurations.First(c => c.Key == "UserName").Value,
                    Password = configurations.First(c => c.Key == "Password").Value,
                    RecurseFolders = Convert.ToBoolean(configurations.First(c => c.Key == "RecurseFolders").Value),
                    IncludeSandbox = Convert.ToBoolean(configurations.First(c => c.Key == "IncludeSandbox").Value),
                    IncludeProduction = Convert.ToBoolean(configurations.First(c => c.Key == "IncludeProduction").Value),
                    ProductionFolder = configurations.First(c => c.Key == "ProductionFolder").Value,
                    SandboxFolder = configurations.First(c => c.Key == "SandboxFolder").Value,
                };

                return View(config);
            }

            return View(new SftpConfigModel());
        }

        [HttpPost]
        public IActionResult UpdateConfig(SftpConfigModel config)
        {
            return RedirectToAction("Index");
        }
    }
}
