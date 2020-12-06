using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;
using SbslFileTransformer.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
