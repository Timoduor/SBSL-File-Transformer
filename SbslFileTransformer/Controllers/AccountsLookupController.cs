using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Controllers
{
    public class AccountsLookupController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountsLookupController(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _hostingEnvironment = env;
        }

        // GET: AccountsLookupController
        public ActionResult Index()
        {
            IEnumerable<AccountsLookup> accounts = _dbContext.Accounts.OrderBy(a => a.Entity).ThenBy(a => a.Number);

            return View(accounts);
        }

        // GET: AccountsLookupController/Create
        public ActionResult Create()
        {
            ViewBag.Entities = new SelectList(new Dictionary<string, string>
            {
                {"IMKE", "IMKE"},
                {"IMRW", "IMRW"},
                {"IMTZ", "IMTZ"}
            }.Select(v => new SelectListItem
            {
                Text = v.Key.ToString(),
                Value = v.Value.ToString()
            }).ToList(), "Value", "Text");

            return View();
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

                    return View(acc);
                }

                if (_dbContext.Accounts.Any())
                {
                    ViewBag.Message = $"An entry with the account number {acc.Number} exists! Try editing it";

                    return View(acc);
                }

                await _dbContext.Accounts.AddAsync(acc);

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(acc);
            }
        }

        // GET: AccountsLookupController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            ViewBag.Entities = new SelectList(new Dictionary<string, string>
            {
                {"IMKE", "IMKE"},
                {"IMRW", "IMRW"},
                {"IMTZ", "IMTZ"}
            }.Select(v => new SelectListItem
            {
                Text = v.Key.ToString(),
                Value = v.Value.ToString()
            }).ToList(), "Value", "Text");

            AccountsLookup account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

            return View("Update", account);
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

                    return View("Update", acc);
                }

                AccountsLookup existing = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == acc.Id);

                if (existing != null)
                {
                    existing.Account = acc.Account;
                    existing.Currency = acc.Currency;
                    existing.Entity = acc.Entity;
                    existing.Name = acc.Name;
                    existing.Number = acc.Number;

                    _dbContext.Entry(existing).State = EntityState.Modified;
                }

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View("Update", acc);
            }
        }

        // GET: AccountsLookupController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            AccountsLookup account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

            if (account != null)
            {
                _dbContext.Remove(account);

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> BulkImport(IFormFile excel)
        {
            if (excel == null || excel.Length == 0)
            {
                return Content("File not properly selected");
            }
            else
            {
                string uploadFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "AccountsUploads");
                Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, excel.FileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await excel.CopyToAsync(stream);
                }

                string failedInserts = await AccountsHelper.ProcessedExcelUpload(filePath, _dbContext);

                if (!string.IsNullOrEmpty(failedInserts))
                {
                    return Content(failedInserts);
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DownloadSample()
        {
            string sampleAccountsFile = Path.Combine(_hostingEnvironment.ContentRootPath, "AccountsUploads", "sample_accounts_format.xlsx");

            byte[] file = await System.IO.File.ReadAllBytesAsync(sampleAccountsFile);

            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "accounts_sample.xlsx");
        }
    }
}