using Licensing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;

namespace SbslFileTransformer.Infrastructure.Licensing.Attributes
{
    /// <summary>
    ///     Feature that can be enabled disabled are All, Admin, POS and WinesAndBeers
    /// </summary>
    public class HandleLicenseAttribute : ActionFilterAttribute
    {
        public HandleLicenseAttribute(string feature)
        {
            Feature = feature;
        }

        private string Feature { get; }

        /// <summary>
        ///     Checks license is not expired otherwise redirects to license entry page
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            bool controllerExempted = filterContext.Controller.GetType()
                .GetCustomAttributes(typeof(LicenseCheckExemptAttribute), false).Length > 0;

            bool actionExempted = filterContext.ActionDescriptor.GetType()
                .GetCustomAttributes(typeof(LicenseCheckExemptAttribute), false).Length > 0;

            if (controllerExempted || actionExempted)
                return;

            LicenseInfo licInfo = new LicenseInfo();

            if (Feature == "All")
            {
                string licensePath = Path.Combine(Directory.GetCurrentDirectory());

                if (licInfo.GetLicenseStatus(out string msg) != LicenseStatus.VALID ||
                    licInfo.License.DoExtraValidation(out string validationMsg) != LicenseStatus.VALID)
                    filterContext.Result = new RedirectResult("/License", false);
            }

            //check login feature is enabled
            if (Feature == "Login")
                if (!licInfo.License.EnableLogin) //feature is disabled
                    filterContext.Result = new RedirectResult("/License/FeatureDisabled", false);

            //check config feature is enabled
            if (Feature == "Config")
                if (!licInfo.License.EnableConfig) //feature is disabled
                    filterContext.Result = new RedirectResult("/License/FeatureDisabled", false);

            //Check update is enabled
            if (Feature == "Update")
                if (!licInfo.License.EnableUpdate) //feature is disabled
                    filterContext.Result = new RedirectResult("/License/FeatureDisabled", false);
        }
    }
}