using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    [HandleLicense("All")]
    public class ConfigController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EncryptionManager _encryptionManager;

        public ConfigController(ApplicationDbContext dbContext, EncryptionManager encryptionManager)
        {
            _dbContext = dbContext;
            _encryptionManager = encryptionManager;
        }

        public async Task<IActionResult> Index()
        {
            var configs = await _dbContext.Configurations.Where(c => c.Key != "Password").OrderBy(c => c.ConfigType).ToListAsync();

            return View(configs);
        }


        public IActionResult RestartService(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
                serviceName = "SBSL ETL Service";

            StaticHelpers.RestartService(serviceName, 120 * 1000);

            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            ViewBag.ConfigTypes = new SelectList(Enum.GetValues(typeof(ConfigurationType)).Cast<ConfigurationType>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList(), "Value", "Text");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Configuration config)
        {
            if (!ModelState.IsValid)
                return View(config);

            //If ConfigType and Key combination exist then it will be UPDATED!!
            await CreateOrUpdate(config);

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Sftp()
        {
            var configurations = await _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToListAsync();

            if (configurations.Count >= 8)
            {

                var config = new SftpConfigModel
                {
                    Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                    Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                    UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                    //Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                    RecurseFolders = true,
                    IncludeSandbox = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeSandbox")?.Value),
                    IncludeProduction = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value),
                    ProductionFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value,
                    SandboxFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value,
                };

                return View(config);
            }

            return View(new SftpConfigModel());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConfig(SftpConfigModel config)
        {
            await UpdateSftp(config);

            return RedirectToAction("Index");
        }

        private async Task UpdateSftp(SftpConfigModel config)
        {
            //update host
            if (!string.IsNullOrEmpty(config.Host))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "Host",
                    Value = config.Host,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.UserName))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "UserName",
                    Value = config.UserName,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.Password))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "Password",
                    Value = _encryptionManager.Encrypt(config.Password),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (config.Port != 0)
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "Port",
                    Value = config.Port.ToString(),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            if (!string.IsNullOrEmpty(config.ProductionFolder))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "ProductionFolder",
                    Value = config.ProductionFolder,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);

                var config2 = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "IncludeProduction",
                    Value = config.IncludeProduction.ToString(),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(config2);
            }

            if (!string.IsNullOrEmpty(config.SandboxFolder))
            {
                var configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "SandboxFolder",
                    Value = config.SandboxFolder,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);

                var config2 = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "IncludeSandbox",
                    Value = config.IncludeSandbox.ToString(),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(config2);
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
