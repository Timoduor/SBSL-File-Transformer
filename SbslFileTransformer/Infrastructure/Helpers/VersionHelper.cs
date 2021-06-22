using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class VersionHelper
    {
        public static string GetVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build).AddSeconds(version.Revision * 2);
            string displayableVersion = $"V:{version}";

            return displayableVersion;
        }
    }
}
