using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using X.PagedList;

namespace SbslFileTransformer.Controllers
{
    public class AccountsLookupController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AccountsLookupController> _logger;

        public AccountsLookupController(ApplicationDbContext dbContext, IWebHostEnvironment env, ILogger<AccountsLookupController> logger)
        {
            this._dbContext = dbContext;
            this._hostingEnvironment = env;
            this._logger = logger;
        }

        // GET: AccountsLookupController
        public ActionResult Index(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 20;

                IEnumerable<AccountsLookup> accounts = this._dbContext.Accounts.OrderBy(a => a.Entity).ThenBy(a => a.Number)
                    .Skip((page - 1) * itemsPerPage).OrderBy(a => a.Entity).ThenBy(a => a.Number).Take(itemsPerPage).ToList();

                count = this._dbContext.UploadedFiles.Count();

                ViewBag.TotalCount = count;

                StaticPagedList<AccountsLookup> pagedList = new StaticPagedList<AccountsLookup>(accounts, page, itemsPerPage, count);

                return this.View(pagedList);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return Content(ex.Message);
            }
        }


        // GET: AccountsLookupController/Create
        public ActionResult Create()
        {
            ViewBag.Entities = new SelectList(new Dictionary<string, string>
            {
                {"IMKE", "IMKE"},
                {"IMRW", "IMRW"},
                {"IMTZ", "IMTZ"},
                {"IMUG", "IMUG"}
            }.Select(v => new SelectListItem
            {
                Text = v.Key.ToString(),
                Value = v.Value.ToString()
            }).ToList(), "Value", "Text");

            return this.View();
        }

        // POST: AccountsLookupController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AccountsLookup acc)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Message = "Invalid values entered!";
                    _logger.LogError("Invalid values entered for accounts lookup!");

                    return this.View(acc);
                }

                if (this._dbContext.Accounts.Any())
                {
                    var message = $"An entry with the account number {acc.Number} exists! Try editing it";

                    ViewBag.Message = message;

                    _logger.LogError(message);

                    return this.View(acc);
                }

                await this._dbContext.Accounts.AddAsync(acc);

                await this._dbContext.SaveChangesAsync();

                return this.RedirectToAction(nameof(this.Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in creating account lookup");
                return this.View(acc);
            }
        }

        // GET: AccountsLookupController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            ViewBag.Entities = new SelectList(new Dictionary<string, string>
            {
                {"IMKE", "IMKE"},
                {"IMRW", "IMRW"},
                {"IMTZ", "IMTZ"},
                {"IMUG", "IMUG"}
            }.Select(v => new SelectListItem
            {
                Text = v.Key.ToString(),
                Value = v.Value.ToString()
            }).ToList(), "Value", "Text");

            AccountsLookup account = await this._dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

            return this.View("Update", account);
        }

        // POST: AccountsLookupController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AccountsLookup acc)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Message = "Invalid values entered!";

                    return this.View("Update", acc);
                }

                AccountsLookup existing = await this._dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == acc.Id);

                if (existing != null)
                {
                    existing.Account = acc.Account;
                    existing.Currency = acc.Currency;
                    existing.Entity = acc.Entity;
                    existing.Name = acc.Name;
                    existing.Number = acc.Number;

                    this._dbContext.Entry(existing).State = EntityState.Modified;
                }

                await this._dbContext.SaveChangesAsync();

                return this.RedirectToAction(nameof(this.Index));
            }
            catch
            {
                return this.View("Update", acc);
            }
        }

        // GET: AccountsLookupController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            AccountsLookup account = await this._dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

            if (account != null)
            {
                this._dbContext.Remove(account);

                await this._dbContext.SaveChangesAsync();
            }

            return this.RedirectToAction("Index");
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
                string uploadFolder = Path.Combine(this._hostingEnvironment.ContentRootPath, "AccountsUploads");
                Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, excel.FileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await excel.CopyToAsync(stream);
                }

                string failedInserts = await UploadHelpers.ProcessedAccountsExcelUpload(filePath, this._dbContext);

                if (!string.IsNullOrEmpty(failedInserts))
                {
                    return this.Content(failedInserts);
                }
            }
            return this.RedirectToAction("Index");
        }

        public async Task<ActionResult> DownloadSample()
        {
            string sampleAccountsFile = Path.Combine(this._hostingEnvironment.ContentRootPath, "AccountsUploads", "sample_accounts_format.xlsx");

            byte[] file = await System.IO.File.ReadAllBytesAsync(sampleAccountsFile);

            return this.File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "accounts_sample.xlsx");
        }
    }
}
