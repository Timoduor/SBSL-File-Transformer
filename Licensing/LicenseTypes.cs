using System.ComponentModel;

namespace Licensing
{
    public enum LicenseTypes
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Single")]
        Single = 1,
        [Description("Volume")]
        Volume = 2
    }
}
