using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

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

            var config = new SftpConfigViewModel
            {
                Host = configurations.First(c => c.Key == "Host").Value,
                Port = Convert.ToInt32(configurations.First(c => c.Key == "Port").Value),
                UserName = configurations.First(c => c.Key == "UserName").Value,
                Password = configurations.First(c => c.Key == "Password").Value
            };

            return View(config);
        }

        [HttpPost]
        public IActionResult UpdateConfig(SftpConfigViewModel config)
        {
            return RedirectToAction("Index");
        }
    }
}
