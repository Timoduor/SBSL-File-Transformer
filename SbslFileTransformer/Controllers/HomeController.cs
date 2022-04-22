using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;
using SbslFileTransformer.Models;
using System;
using System.Diagnostics;
using System.Linq;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Controllers
{
    //[HandleLicense("All")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, IFileProvider fileProvider)
        {
            this._logger = logger;
            this._fileProvider = fileProvider;
        }

        [LicenseCheckExempt]
        public IActionResult Index()
        {
            return this.View();
        }

        public IActionResult Eula()
        {
            try
            {
                IFileInfo file = this._fileProvider.GetDirectoryContents("Content").FirstOrDefault(f => f.Name == "eula.docx");

                return this.File(file.CreateReadStream(),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Index");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}