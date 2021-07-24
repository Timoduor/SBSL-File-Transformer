using Licensing;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SbslFileTransformer.Infrastructure.Licensing
{
    public class ETLLicense : LicenseEntity
    {
        public ETLLicense()
        {
            //Initialize app name for the license
            AppName = "SBSLETL";
        }

        [DisplayName("Enable All")]
        [Category("License Options")]
        [XmlElement("EnableAll")]
        [ShowInLicenseInfo(true, "Enable All", ShowInLicenseInfoAttribute.FormatType.String)]
        public bool EnableAll { get; set; }

        [DisplayName("Enable Login")]
        [Category("License Options")]
        [XmlElement("EnableLogin")]
        [ShowInLicenseInfo(true, "Enable Login", ShowInLicenseInfoAttribute.FormatType.String)]
        public bool EnableLogin { get; set; }


        [DisplayName("Enable Config")]
        [Category("License Options")]
        [XmlElement("EnableConfig")]
        [ShowInLicenseInfo(true, "Enable Config", ShowInLicenseInfoAttribute.FormatType.String)]
        public bool EnableConfig { get; set; }

        [DisplayName("Enable Update")]
        [Category("License Options")]
        [XmlElement("EnableUpdate")]
        [ShowInLicenseInfo(true, "Enable Update", ShowInLicenseInfoAttribute.FormatType.String)]
        public bool EnableUpdate { get; set; }

        public override LicenseStatus DoExtraValidation(out string validationMsg)
        {
            LicenseStatus licStatus;

            validationMsg = string.Empty;

            switch (Type)
            {
                case LicenseTypes.Single:
                    //For Single License, check whether UID is matched
                    if (UID == LicenseHandler.GenerateUID(AppName))
                    {
                        licStatus = LicenseStatus.VALID;
                    }
                    else
                    {
                        validationMsg = "The license is NOT for this copy!";
                        licStatus = LicenseStatus.INVALID;
                    }

                    break;

                case LicenseTypes.Volume:
                    //No UID checking for Volume License
                    licStatus = LicenseStatus.VALID;
                    break;

                default:
                    validationMsg = "Invalid license";
                    licStatus = LicenseStatus.INVALID;
                    break;
            }

            if (ExpiryDate < DateTime.Now)
            {
                validationMsg = "The license has expired";
                licStatus = LicenseStatus.EXPIRED;
            }

            return licStatus;
        }
    }
}