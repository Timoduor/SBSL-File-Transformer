using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    //[HandleLicense("All")]
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
            var configurations = await _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report).ToListAsync();

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
            await UpdateReportConfig(config);

            return RedirectToAction("Index", "Config");
        }

        public async Task<IActionResult> EmailGroups()
        {
            var groups = await _dbContext.EmailGroups.ToListAsync();

            return View(groups);
        }

        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(EmailGroup group)
        {
            _dbContext.EmailGroups.Add(group);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("EmailGroups");
        }

        public async Task<IActionResult> EditGroup(int id)
        {
            var group = await _dbContext.EmailGroups.FindAsync(id);

            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> EditGroup(EmailGroup group)
        {
            _dbContext.Update(group);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("EmailGroups");
        }

        public async Task<IActionResult> Deactivate(int id, bool active)
        {
            var group = await _dbContext.EmailGroups.FindAsync(id);

            group.IsActive = active;

            _dbContext.Update(group);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("EmailGroups");
        }

        private async Task UpdateReportConfig(ReportConfigModel config)
        {
            if (!string.IsNullOrEmpty(config.BaseUrl))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "BaseUrl",
                    Value = config.BaseUrl,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ClientId))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "ClientId",
                    Value = config.ClientId,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ClientSecret))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "ClientSecret",
                    Value = config.ClientSecret,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.EmailBody))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "EmailBody",
                    Value = config.EmailBody,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.EmailHeader))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "EmailHeader",
                    Value = config.EmailHeader,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.EnvironmentUrl))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "EnvironmentUrl",
                    Value = config.EnvironmentUrl,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ExportType))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "ExportType",
                    Value = config.ExportType,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.Password))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "Password",
                    Value = config.Password,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.Scope))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "Scope",
                    Value = config.Scope,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.TokenUrl))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "TokenUrl",
                    Value = config.TokenUrl,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.UserName))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "UserName",
                    Value = config.UserName,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.UserToken))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "UserToken",
                    Value = config.UserToken,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

        }

        private async Task CreateOrUpdate(Configuration config)
        {

            var existing = await _dbContext.Configurations.FirstOrDefaultAsync(c => c.Key.ToLower() == config.Key.ToLower() && c.ConfigType == config.ConfigType);

            if (existing != null)
            {
                existing.Value = config.Value;
                existing.Updated = DateTime.Now;

                _dbContext.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                config.Updated = DateTime.Now;
                _dbContext.Add(config);
            }

            await _dbContext.SaveChangesAsync();
        }


    }
}
