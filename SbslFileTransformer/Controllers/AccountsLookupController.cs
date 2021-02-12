using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Controllers
{
    public class AccountsLookupController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountsLookupController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: AccountsLookupController
        public ActionResult Index()
        {
            var accounts = _dbContext.Accounts;

            return View(accounts);
        }

        // GET: AccountsLookupController/Create
        public ActionResult Create()
        {
            ViewBag.Entities = new SelectList(new Dictionary<string, string>
            {
                { "IMKE", "IMKE" },
                { "IMRW", "IMRW" },
                { "IMTZ", "IMTZ" },
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
                    @ViewBag.Message = "Invalid values entered!";

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
                { "IMKE", "IMKE" },
                { "IMRW", "IMRW" },
                { "IMTZ", "IMTZ" },
            }.Select(v => new SelectListItem
            {
                Text = v.Key.ToString(),
                Value = v.Value.ToString()
            }).ToList(), "Value", "Text");

            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

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
                    @ViewBag.Message = "Invalid values entered!";

                    return View("Update", acc);
                }

                var existing = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == acc.Id);

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
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);

            if (account != null)
            {
                _dbContext.Remove(account);

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
