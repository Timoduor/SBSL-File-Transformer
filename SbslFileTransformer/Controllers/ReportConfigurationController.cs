using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Controllers
{
    public class ReportConfigurationController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ReportConfigurationController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ReportConfigurationController(IWebHostEnvironment env, ILogger<ReportConfigurationController> logger,
            ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
            this._logger = logger;
            this._hostingEnvironment = env;
        }

        // GET: ReportConfigurationController
        public ActionResult Index()
        {
            var configs = _dbContext.ReportConfigurations.OrderByDescending(x => x.Id).ToList();

            return View(configs);
        }

        // GET: ReportConfigurationController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var config = await _dbContext.ReportConfigurations.FindAsync(id);

            return View(config);
        }

        // GET: ReportConfigurationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReportConfigurationController/Create
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating report config", ex);
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ReportConfigurationController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReportConfigurationController/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error editing report config", ex);
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> BulkImport(IFormFile excel)
        {
            if (excel == null || excel.Length == 0)
            {
                return this.Content("File not properly selected");
            }
            else
            {
                string uploadFolder = Path.Combine(this._hostingEnvironment.ContentRootPath, "EscalationUploads");
                Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, excel.FileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await excel.CopyToAsync(stream);
                }

                string failedInserts = await UploadHelpers.ProcessEscalationsExcelUpload(filePath, this._dbContext);

                if (!string.IsNullOrEmpty(failedInserts))
                {
                    return this.Content(failedInserts);
                }
            }
            return this.RedirectToAction("Index");
        }

        public async Task<ActionResult> DownloadSample()
        {
            string sampleAccountsFile = Path.Combine(this._hostingEnvironment.ContentRootPath, "EscalationUploads", "sample_escalations.xlsx");

            byte[] file = await System.IO.File.ReadAllBytesAsync(sampleAccountsFile);

            return this.File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sample_escalations.xlsx");
        }

        // POST: ReportConfigurationController/Delete/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Disable(IFormCollection collection)
        {
            try
            {
                 var report = await _dbContext.ReportConfigurations.FindAsync(Convert.ToInt32(collection["id"]));

                report.IsEnabled = Convert.ToBoolean(collection["IsEnabled"]);

                _dbContext.Update(report);

                await _dbContext.SaveChangesAsync();                
            }
            catch(Exception ex)
            {
                _logger.LogError("Error saving report config enable/disable state", ex);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ReportConfigurationController/Delete/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var report = await _dbContext.ReportConfigurations.FindAsync(id);

                _dbContext.Remove(report);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving report config enable/disable state", ex);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
