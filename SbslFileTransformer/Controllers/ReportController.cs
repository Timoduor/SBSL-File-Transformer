using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    [HandleLicense("All")]
    [AllowAnonymous]
    public class ReportController : Controller
    {
        private ApplicationDbContext _dbContext;
        private ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger, ApplicationDbContext dbContext)//, PluginManager pluginManager)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var configurations = _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report).ToList();

            var config = new ReportConfigModel
            {
                BaseUrl = configurations.FirstOrDefault(c => c.Key == "BaseUrl")?.Value,
                EnvironmentUrl = configurations.FirstOrDefault(c => c.Key == "EnvironmentUrl")?.Value,
                UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                UserToken = configurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                EmailBody = configurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                EmailHeader = configurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                ExportType = configurations.FirstOrDefault(c => c.Key == "ExportType")?.Value,
            };


            return View(config);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ReportConfigModel config)
        {
            return RedirectToAction("Index", "Config");
        }

        public async Task<IActionResult> EmailGroups()
        {
            var groups = await _dbContext.EmailGroups.ToListAsync();

            return View(groups);
        }

        public async Task<IActionResult> EditGroup(int groupId)
        {
            var group = await _dbContext.EmailGroups.FindAsync(groupId);

            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> EditGroup(EmailGroup group)
        {
            _dbContext.Update(group);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }


    }
}
