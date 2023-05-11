using System.IO;
using System.Linq;
using Licensing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using SbslFileTransformer.Infrastructure.Licensing;
using SbslFileTransformer.Infrastructure.Licensing.Attributes;

namespace SbslFileTransformer.Controllers
{
    [LicenseCheckExempt]
    [AllowAnonymous]
    public class LicenseController : Controller
    {
        private readonly IFileProvider _fileProvider;

        public LicenseController(IFileProvider fileProvider)
        {
            this._fileProvider = fileProvider;
        }

        // GET: License
        public ActionResult Index()
        {
            //displays the license info if it is ok else redirects to renew license
            LicenseInfo licenseInfo = new LicenseInfo();

            string licensePath = this._fileProvider.GetDirectoryContents("/").FirstOrDefault(f => f.Name == "license.lic")
                ?.PhysicalPath;

            LicenseStatus status = licenseInfo.GetLicenseStatus(out string licenseMessage);

            ViewBag.LicenseMessage = licenseMessage;
            ViewBag.Status = status;

            string validationMsg = string.Empty;

            if (status == LicenseStatus.VALID &&
                licenseInfo.License.DoExtraValidation(out validationMsg) == LicenseStatus.VALID)
            {
                ViewBag.License = licenseInfo.License;
                ViewBag.LicenseInfo = licenseInfo.GetLicenseInfo(licenseInfo.License, "");
            }
            else
            {
                ViewBag.LicenseMessage += validationMsg;

                return this.RedirectToAction("RenewLicense");
            }

            return this.View();
        }

        public ActionResult RenewLicense()
        {
            //displays the current UID for this machine
            LicenseActivator activator = new LicenseActivator();
            ViewBag.Uid = activator.GenerateUID();

            LicenseInfo licenseInfo = new LicenseInfo();

            LicenseStatus status = licenseInfo.GetLicenseStatus(out string licenseMessage);

            ViewBag.LicenseMessage = licenseMessage;
            ViewBag.Status = status;

            return this.View();
        }

        [HttpPost]
        public ActionResult RenewLicense(string licKey)
        {
            LicenseActivator licActivator = new LicenseActivator();

            if (licActivator.ValidateLicense(licKey, out string msg, out LicenseStatus status))
            {
                string licensePath = this._fileProvider.GetDirectoryContents("/").FirstOrDefault(f => f.Name == "license.lic")
                    ?.PhysicalPath;

                if (licensePath == null) licensePath = Path.Combine(Directory.GetCurrentDirectory(), "license.lic");

                System.IO.File.WriteAllText(licensePath, licKey);
                return this.RedirectToAction("Index");
            }

            ViewBag.LicStatus = status;
            ViewBag.Uid = licActivator.GenerateUID();
            ViewBag.Message = msg;

            return this.View();
        }

        public ActionResult FeatureDisabled()
        {
            return this.View();
        }
    }
}