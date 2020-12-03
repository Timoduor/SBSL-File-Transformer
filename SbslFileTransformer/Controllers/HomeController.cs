using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;
using SbslFileTransformer.Models;
using System;
using System.Diagnostics;
using System.Linq;

namespace SbslFileTransformer.Controllers
{
    [HandleLicense("All")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileProvider _fileProvider;


        public HomeController(ILogger<HomeController> logger, IFileProvider fileProvider)
        {
            _logger = logger;
            _fileProvider = fileProvider;
        }

        [LicenseCheckExempt]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Config()
        {
            return View();
        }

        public IActionResult Logs()
        {
            try
            {
                var files = _fileProvider.GetDirectoryContents("logs");

                var latestFiles =
                          files
                          .OrderByDescending(f => f.LastModified);

                return View(latestFiles);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        public IActionResult DownloadLogFile(string name)
        {
            try
            {
                var files = _fileProvider.GetDirectoryContents("logs");

                var file = files.FirstOrDefault(f => f.Name == name);

                return File(file.CreateReadStream(), "text/plain");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return RedirectToAction("Logs");
            }
        }

        public IActionResult Eula()
        {
            try
            {
                var file = _fileProvider.GetDirectoryContents("Content").FirstOrDefault(f => f.Name == "eula.docx");

                return File(file.CreateReadStream(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
