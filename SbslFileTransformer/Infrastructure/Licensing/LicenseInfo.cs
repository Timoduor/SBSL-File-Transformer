using Licensing;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Licensing
{
    public class LicenseInfo
    {
        byte[] _certPubicKeyData;

        public ETLLicense License { private set; get; }

        public string DateFormat { get; set; }

        public string DateTimeFormat { get; set; }

        public LicenseStatus GetLicenseStatus(out string msg)
        {
            //Initialize variables with default values
            msg = string.Empty;

            //Read public key from assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (MemoryStream mem = new MemoryStream())
            {
                assembly.GetManifestResourceStream("SbslFileTransformer.LicenseVerify.cer")?.CopyTo(mem);

                _certPubicKeyData = mem.ToArray();
            }

            LicenseStatus status;

            var licensePath = Path.Combine(Directory.GetCurrentDirectory(), "license.lic");

            //Check if the XML license file exists
            if (File.Exists(licensePath))
            {
                License = (ETLLicense)LicenseHandler.ParseLicenseFromBASE64String(
                    typeof(ETLLicense),
                    File.ReadAllText(licensePath),
                    _certPubicKeyData,
                    out status,
                    out msg);
            }
            else
            {
                status = LicenseStatus.INVALID;
                msg = "Your copy of this application is not activated";
            }

            //TODO perform additional checks for properties to like enabled features
            return status;
        }

        public string GetLicenseInfo(ETLLicense license, string additionalInfo)
        {
            try
            {
                StringBuilder sb = new StringBuilder(512);

                Type typeLic = license.GetType();
                PropertyInfo[] props = typeLic.GetProperties();

                object _value = null;
                string formattedValue = string.Empty;
                foreach (PropertyInfo p in props)
                {
                    try
                    {
                        ShowInLicenseInfoAttribute showAttr = (ShowInLicenseInfoAttribute)Attribute.GetCustomAttribute(p, typeof(ShowInLicenseInfoAttribute));
                        if (showAttr != null && showAttr.ShowInLicenseInfo)
                        {
                            _value = p.GetValue(license, null);
                            sb.Append(showAttr.DisplayAs);
                            sb.Append(": ");

                            //Append value and apply the format
                            if (_value != null)
                            {
                                switch (showAttr.DataFormatType)
                                {
                                    case ShowInLicenseInfoAttribute.FormatType.String:
                                        formattedValue = _value.ToString();
                                        break;

                                    case ShowInLicenseInfoAttribute.FormatType.Date:
                                        if (p.PropertyType == typeof(DateTime) && !string.IsNullOrWhiteSpace(DateFormat))
                                        {
                                            formattedValue = ((DateTime)_value).ToString(DateFormat);
                                        }
                                        else
                                        {
                                            formattedValue = _value.ToString();
                                        }
                                        break;

                                    case ShowInLicenseInfoAttribute.FormatType.DateTime:
                                        if (p.PropertyType == typeof(DateTime) && !string.IsNullOrWhiteSpace(DateTimeFormat))
                                        {
                                            formattedValue = ((DateTime)_value).ToString(DateTimeFormat);
                                        }
                                        else
                                        {
                                            formattedValue = _value.ToString();
                                        }
                                        break;

                                    case ShowInLicenseInfoAttribute.FormatType.EnumDescription:
                                        string name = Enum.GetName(p.PropertyType, _value);
                                        if (name != null)
                                        {
                                            FieldInfo fi = p.PropertyType.GetField(name);

                                            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

                                            formattedValue = dna != null ? dna.Description : _value.ToString();
                                        }
                                        else
                                        {
                                            formattedValue = _value.ToString();
                                        }
                                        break;
                                }

                                sb.Append(formattedValue);
                            }

                            sb.Append("\r\n");
                        }
                    }
                    catch
                    {
                        //Ignore exeption
                    }
                }


                if (!string.IsNullOrWhiteSpace(additionalInfo))
                {
                    sb.Append(additionalInfo.Trim());
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}