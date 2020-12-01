using Licensing;
using System;
using System.IO;
using System.Reflection;

namespace SbslFileTransformer.Infrastructure.Licensing
{
    internal class LicenseActivator : IDisposable
    {
        public string AppName { get; set; }

        public byte[] CertificatePublicKeyData { private get; set; }

        public Type LicenseObjectType { get; set; }

        public LicenseActivator()
        {
            AppName = "SBSLETL";

            LicenseObjectType = typeof(ETLLicense);

            //Read public key from assembly
            var assembly = Assembly.GetExecutingAssembly();
            using (MemoryStream mem = new MemoryStream())
            {
                assembly.GetManifestResourceStream("SbslFileTransformer.LicenseVerify.cer")?.CopyTo(mem);

                CertificatePublicKeyData = mem.ToArray();
            }
        }

        public bool ValidateLicense(string licenseText, out string msg, out LicenseStatus licStatus)
        {
            if (string.IsNullOrWhiteSpace(licenseText))
            {
                msg = "Please input a valid license";
                licStatus = LicenseStatus.UNDEFINED;
                return false;
            }

            //Check the activation string
            LicenseHandler.ParseLicenseFromBASE64String(LicenseObjectType, licenseText, CertificatePublicKeyData, out licStatus, out msg);

            switch (licStatus)
            {
                case LicenseStatus.VALID:
                    msg = "License is valid";
                    return true;

                case LicenseStatus.CRACKED:
                case LicenseStatus.INVALID:
                case LicenseStatus.UNDEFINED:
                case LicenseStatus.EXPIRED:
                    msg += " License is INVALID";
                    return false;

                default:
                    return false;
            }
        }

        public string GenerateUID()
        {
            return LicenseHandler.GenerateUID(AppName);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}