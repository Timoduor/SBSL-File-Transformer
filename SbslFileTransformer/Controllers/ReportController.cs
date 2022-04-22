using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using SbslFileTransformer.Models.ViewModels;
using X.PagedList;

namespace SbslFileTransformer.Controllers
{
    //[HandleLicense("All")]
    [AllowAnonymous]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger,
            ApplicationDbContext dbContext) //, PluginManager pluginManager)
        {
            this._dbContext = dbContext;
            this._logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            System.Collections.Generic.List<Configuration> configurations = await this._dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report)
                .ToListAsync();

            ReportConfigModel config = new ReportConfigModel
            {
                BaseUrl = configurations.FirstOrDefault(c => c.Key == "BaseUrl")?.Value,
                EnvironmentUrl = configurations.FirstOrDefault(c => c.Key == "EnvironmentUrl")?.Value,
                UserToken = configurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                EmailBody = configurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                EmailHeader = configurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                ExportType = configurations.FirstOrDefault(c => c.Key == "ExportType")?.Value
            };


            return this.View(config);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ReportConfigModel config)
        {
            await this.UpdateReportConfig(config);

            return this.RedirectToAction("Index", "Config");
        }

        public async Task<IActionResult> EmailGroups()
        {
            System.Collections.Generic.List<EmailGroup> groups = await this._dbContext.EmailGroups.ToListAsync();

            return this.View(groups);
        }

        public IActionResult CreateGroup()
        {
            ViewBag.Countries = new SelectList(Enum.GetValues(typeof(Country)).Cast<Country>().Select(v =>
                new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            ViewBag.Sprints = new SelectList(Enum.GetValues(typeof(Sprint)).Cast<Sprint>().Select(v =>
                new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            ViewBag.Categories = new SelectList(Enum.GetValues(typeof(ReportCategory)).Cast<ReportCategory>().Select(
                v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(EmailGroup group)
        {
            this._dbContext.EmailGroups.Add(group);

            await this._dbContext.SaveChangesAsync();

            return this.RedirectToAction("EmailGroups");
        }

        public async Task<IActionResult> EditGroup(int id)
        {
            EmailGroup group = await this._dbContext.EmailGroups.FindAsync(id);

            ViewBag.Countries = new SelectList(Enum.GetValues(typeof(Country)).Cast<Country>().Select(v =>
                new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            ViewBag.Sprints = new SelectList(Enum.GetValues(typeof(Sprint)).Cast<Sprint>().Select(v =>
                new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            ViewBag.Categories = new SelectList(Enum.GetValues(typeof(ReportCategory)).Cast<ReportCategory>().Select(
                v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");


            return this.View(group);
        }

        [HttpPost]
        public async Task<IActionResult> EditGroup(EmailGroup group)
        {
            this._dbContext.Update(group);

            await this._dbContext.SaveChangesAsync();

            return this.RedirectToAction("EmailGroups");
        }

        public async Task<IActionResult> Deactivate(int id, bool active)
        {
            EmailGroup group = await this._dbContext.EmailGroups.FindAsync(id);

            group.IsActive = active;

            this._dbContext.Update(group);

            await this._dbContext.SaveChangesAsync();

            return this.RedirectToAction("EmailGroups");
        }

        public async Task<IActionResult> Processed(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 10;

                IOrderedEnumerable<ProcessedReport> uploadedFiles = this._dbContext.ProcessedReports.OrderByDescending(f => f.ProcessedDate)
                    .Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList()
                    .OrderByDescending(f => f.ProcessedDate);

                count = await this._dbContext.ProcessedReports.CountAsync();

                StaticPagedList<ProcessedReport> pagedList = new StaticPagedList<ProcessedReport>(uploadedFiles, page, itemsPerPage, count);

                return this.View(pagedList);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Index", "Home");
            }
        }

        private async Task UpdateReportConfig(ReportConfigModel config)
        {
            if (!string.IsNullOrEmpty(config.BaseUrl))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "BaseUrl",
                    Value = config.BaseUrl,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ClientId))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "ClientId",
                    Value = config.ClientId,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ClientSecret))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "ClientSecret",
                    Value = config.ClientSecret,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.EmailBody))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "EmailBody",
                    Value = config.EmailBody,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.EmailHeader))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "EmailHeader",
                    Value = config.EmailHeader,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.EnvironmentUrl))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "EnvironmentUrl",
                    Value = config.EnvironmentUrl,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ExportType))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "ExportType",
                    Value = config.ExportType,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.Scope))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "Scope",
                    Value = config.Scope,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.TokenUrl))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "TokenUrl",
                    Value = config.TokenUrl,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.UserToken))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Report,
                    Key = "UserToken",
                    Value = config.UserToken,
                    Updated = DateTime.Now
                };

                await this.CreateOrUpdate(configuration);
            }
        }

        private async Task CreateOrUpdate(Configuration config)
        {
            Configuration existing = await this._dbContext.Configurations.FirstOrDefaultAsync(c =>
                c.Key.ToLower() == config.Key.ToLower() && c.ConfigType == config.ConfigType);

            if (existing != null)
            {
                existing.Value = config.Value;
                existing.Updated = DateTime.Now;

                this._dbContext.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                config.Updated = DateTime.Now;
                this._dbContext.Add(config);
            }

            await this._dbContext.SaveChangesAsync();
        }
    }
}