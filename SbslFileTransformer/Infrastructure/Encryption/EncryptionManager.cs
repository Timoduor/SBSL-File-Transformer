using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;

namespace SbslFileTransformer.Infrastructure.Encryption
{
    public class EncryptionManager
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<EncryptionManager> _logger;

        private readonly static object _locker = new object();

        private string Purpose = "Encrypt SFTP Password";

        public EncryptionManager(IDataProtectionProvider dataProtectionProvider, ILogger<EncryptionManager> logger)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;

            //if(Key == "9a3230c9-191c-4d9d-b803-4bab3d96888a")
            //    _logger.LogWarning("Please change the default encryption/decryption key");

            //var configKey = configuration.GetSection("EnKey").Value;

            //Key = string.IsNullOrEmpty(configKey) ? "9a3230c9-191c-4d9d-b803-4bab3d96888a" : configKey;

        }

        public string Encrypt(string input = "")
        {
            var protector = _dataProtectionProvider.CreateProtector(Purpose);
            return protector.Protect(input);
        }

        public string Decrypt(string cipherText = "")
        {
            var protector = _dataProtectionProvider.CreateProtector(Purpose);
            return protector.Unprotect(cipherText);
        }

        public string GetMd5(string filePath)
        {
            lock (_locker)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = md5.ComputeHash(stream);

                        stream.Close();

                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }
    }
}
