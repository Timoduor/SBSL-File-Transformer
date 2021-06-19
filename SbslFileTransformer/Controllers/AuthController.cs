using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Models;

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
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            ViewData["Title"] = "Login";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View();

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Username) ??
                           await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        TempData["ErrorMessage"] = "Please confirm your email address to log in.";
                        return View(model);
                    }


                    if (!user.IsEnabled || user.IsDeleted)
                    {
                        TempData["ErrorMessage"] =
                            $"Problem with user login for '{model.Username}'. Please contact admin for assistance";
                        return View(model);
                    }

                    var result =
                        await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        return RedirectToAction("Index", "Home");
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
                _logger.LogError(ex, "Problem logging in!");
                TempData["ErrorMessage"] = "Error occured log in. Contact support for help";
            }

            return RedirectToAction("Login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }
    }
}