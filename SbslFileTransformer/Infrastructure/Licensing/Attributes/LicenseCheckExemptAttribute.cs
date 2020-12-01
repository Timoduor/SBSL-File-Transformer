

using Microsoft.AspNetCore.Mvc.Filters;

namespace SbslFileTransformer.Infrastructure.Licensing.Attributes
{
    /// <summary>
    /// To exempt certain controller from being checked for licensing
    /// </summary>
    public class LicenseCheckExemptAttribute : ActionFilterAttribute
    {

    }
}