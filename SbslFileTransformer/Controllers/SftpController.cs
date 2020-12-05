using Microsoft.AspNetCore.Mvc;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Linq;

namespace SbslFileTransformer.Controllers
{
    public class SftpController : Controller
    {
        private ApplicationDbContext _dbContext;

        public SftpController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var configurations = _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

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

        [HttpPost]
        public IActionResult UpdateConfig(SftpConfigModel config)
        {
            return RedirectToAction("Index");
        }
    }
}
