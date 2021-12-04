using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    //[HandleLicense("All")]
    public class ConfigController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmailSender _emailSender;
        private readonly EncryptionManager _encryptionManager;

        public ConfigController(ApplicationDbContext dbContext, EncryptionManager encryptionManager,
            EmailSender emailSender)
        {
            _dbContext = dbContext;
            _encryptionManager = encryptionManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            System.Collections.Generic.List<Configuration> configs = await _dbContext.Configurations.Where(c => c.Key != "Password").OrderBy(c => c.ConfigType).OrderBy(c => c.ConfigType)
                .ToListAsync();

            ViewBag.ServiceName = configs
                .FirstOrDefault(c => c.Key == "ServiceName" && c.ConfigType == ConfigurationType.Service).Value;

            return View(configs);
        }


        public IActionResult RestartService(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
                serviceName = _dbContext.Configurations
                    .First(c => c.Key == "ServiceName" && c.ConfigType == ConfigurationType.Service).Value;

            if (string.IsNullOrEmpty(serviceName))
                serviceName = "SBSL ETL Service";

            FileHelpers.RestartService();

            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            ViewBag.ConfigTypes = new SelectList(Enum.GetValues(typeof(ConfigurationType)).Cast<ConfigurationType>()
                .Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            return View();
        }

        public IActionResult Update(int configType, string key)
        {
            Configuration config =
                _dbContext.Configurations.FirstOrDefault(c =>
                    c.ConfigType == (ConfigurationType)configType && c.Key == key);

            ViewBag.ConfigTypes = new SelectList(Enum.GetValues(typeof(ConfigurationType)).Cast<ConfigurationType>()
                .Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text", configType);

            return View(config);
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
            System.Collections.Generic.List<Configuration> configurations = await _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                .ToListAsync();

            if (configurations.Count >= 8)
            {
                SftpConfigModel config = new SftpConfigModel
                {
                    Host = configurations.FirstOrDefault(c => c.Key == "Host" && c.ConfigType == ConfigurationType.Sftp)
                        ?.Value,
                    Port = Convert.ToInt32(configurations
                        .FirstOrDefault(c => c.Key == "Port" && c.ConfigType == ConfigurationType.Sftp)?.Value),
                    UserName = configurations
                        .FirstOrDefault(c => c.Key == "UserName" && c.ConfigType == ConfigurationType.Sftp)?.Value,
                    //Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                    RecurseFolders = true,
                    IncludeSandbox = Convert.ToBoolean(configurations.FirstOrDefault(c =>
                        c.Key == "IncludeSandbox" && c.ConfigType == ConfigurationType.Sftp)?.Value),
                    IncludeProduction = Convert.ToBoolean(configurations.FirstOrDefault(c =>
                        c.Key == "IncludeProduction" && c.ConfigType == ConfigurationType.Sftp)?.Value),
                    ProductionFolder = configurations.FirstOrDefault(c =>
                        c.Key == "ProductionFolder" && c.ConfigType == ConfigurationType.Sftp)?.Value,
                    SandboxFolder = configurations
                        .FirstOrDefault(c => c.Key == "SandboxFolder" && c.ConfigType == ConfigurationType.Sftp)?.Value
                };

                return View(config);
            }

            return View(new SftpConfigModel());
        }

        public async Task<IActionResult> Smtp()
        {
            System.Collections.Generic.List<Configuration> configurations = await _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Email)
                .ToListAsync();

            if (configurations.Count >= 5)
            {
                SmtpConfigModel config = new SmtpConfigModel
                {
                    EmailAddress = configurations
                        .FirstOrDefault(c => c.Key == "EmailAddress" && c.ConfigType == ConfigurationType.Email)?.Value,
                    Port = Convert.ToInt32(configurations
                        .FirstOrDefault(c => c.Key == "Port" && c.ConfigType == ConfigurationType.Email)?.Value),
                    UserName = configurations
                        .FirstOrDefault(c => c.Key == "UserName" && c.ConfigType == ConfigurationType.Email)?.Value,
                    //Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                    SmtpServer = configurations
                        .FirstOrDefault(c => c.Key == "SmtpServer" && c.ConfigType == ConfigurationType.Email)?.Value,
                    Name = configurations
                        .FirstOrDefault(c => c.Key == "Name" && c.ConfigType == ConfigurationType.Email)?.Value,
                    Recipients = configurations
                        .FirstOrDefault(c => c.Key == "Recipients" && c.ConfigType == ConfigurationType.Email)?.Value,
                    UseSsl = Convert.ToBoolean(configurations
                        .FirstOrDefault(c => c.Key == "UseSsl" && c.ConfigType == ConfigurationType.Email)?.Value),
                    UseDefaultCredentials = Convert.ToBoolean(configurations.FirstOrDefault(c =>
                        c.Key == "UseDefaultCredentials" && c.ConfigType == ConfigurationType.Email)?.Value)
                };

                return View(config);
            }

            return View(new SmtpConfigModel());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSmtpConfig(SmtpConfigModel config)
        {
            await UpdateSmtp(config);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConfig(SftpConfigModel config)
        {
            await UpdateSftp(config);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendTestEmail()
        {
            System.Collections.Generic.List<string> testFiles = Directory
                .GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "*.*",
                    new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive })
                .Take(2).ToList();

            for (int i = 0; i < testFiles.Count; i++)
            {
                string newFileName = Path.ChangeExtension(testFiles[i], ".txt");

                if (!System.IO.File.Exists(newFileName)) System.IO.File.Copy(testFiles[i], newFileName);

                testFiles[i] = Path.ChangeExtension(testFiles[i], ".txt");
            }

            await _emailSender.SendMessage(null, "Test Email from Windows Box",
                "This is to confirm that the windows box can send emails with attachments", false, testFiles);

            return RedirectToAction("Smtp");
        }


        private async Task UpdateSmtp(SmtpConfigModel config)
        {
            //update host
            if (!string.IsNullOrEmpty(config.UserName))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "UserName",
                    Value = config.UserName,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.Password))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "Password",
                    Value = _encryptionManager.Encrypt(config.Password),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            //if (!string.IsNullOrEmpty(config.UseSsl))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "UseSsl",
                    Value = config.UseSsl.ToString(),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "UseDefaultCredentials",
                    Value = config.UseDefaultCredentials.ToString(),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (config.Port != 0)
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "Port",
                    Value = config.Port.ToString(),
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.Name))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "Name",
                    Value = config.Name,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.SmtpServer))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "SmtpServer",
                    Value = config.SmtpServer,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.EmailAddress))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "EmailAddress",
                    Value = config.EmailAddress,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (!string.IsNullOrEmpty(config.Recipients))
            {
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Email,
                    Key = "Recipients",
                    Value = config.Recipients,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }
        }

        private async Task UpdateSftp(SftpConfigModel config)
        {
            //update host
            if (!string.IsNullOrEmpty(config.Host))
            {
                Configuration configuration = new Configuration
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
                Configuration configuration = new Configuration
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
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "Password",
                    Value = config.Password,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);
            }

            //update host
            if (config.Port != 0)
            {
                Configuration configuration = new Configuration
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
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "ProductionFolder",
                    Value = config.ProductionFolder,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);

                Configuration config2 = new Configuration
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
                Configuration configuration = new Configuration
                {
                    ConfigType = ConfigurationType.Sftp,
                    Key = "SandboxFolder",
                    Value = config.SandboxFolder,
                    Updated = DateTime.Now
                };

                await CreateOrUpdate(configuration);

                Configuration config2 = new Configuration
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
            Configuration existing = await _dbContext.Configurations.FirstOrDefaultAsync(c =>
                c.Key.ToLower() == config.Key.ToLower() && c.ConfigType == config.ConfigType);

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