using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Models;
using System;
using System.Threading.Tasks;
using SbslFileTransformer.Models.AppUser;

namespace SbslFileTransformer.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(ILogger<AuthController> logger, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            this._logger = logger;
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            ViewData["Title"] = "Login";
            return this.View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return this.View();

            try
            {
                ApplicationUser user = await this._userManager.FindByEmailAsync(model.Username) ??
                           await this._userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    if (!await this._userManager.IsEmailConfirmedAsync(user))
                    {
                        TempData["ErrorMessage"] = "Please confirm your email address to log in.";
                        return this.View(model);
                    }


                    if (!user.IsEnabled || user.IsDeleted)
                    {
                        TempData["ErrorMessage"] =
                            $"Problem with user login for '{model.Username}'. Please contact admin for assistance";
                        return this.View(model);
                    }

                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await this._signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return this.Redirect(returnUrl);
                        return this.RedirectToAction("Index", "Home");
                    }

                    TempData["ErrorMessage"] = "Invalid username or password";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Problem logging in!");
                TempData["ErrorMessage"] = "Error occured log in. Contact support for help";
            }

            return this.RedirectToAction("Login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();

            return this.RedirectToAction("Login");
        }
    }
}