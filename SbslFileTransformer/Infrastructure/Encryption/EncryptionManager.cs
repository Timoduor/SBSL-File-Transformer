using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;

namespace SbslFileTransformer.Infrastructure.Encryption
{
    public class EncryptionManager
    {
        private static readonly object _locker = new object();
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<EncryptionManager> _logger;

        private readonly string Purpose = "Encrypt SFTP Password";

        public EncryptionManager(IDataProtectionProvider dataProtectionProvider, ILogger<EncryptionManager> logger)
        {
            this._dataProtectionProvider = dataProtectionProvider;
            this._logger = logger;

            //if(Key == "9a3230c9-191c-4d9d-b803-4bab3d96888a")
            //    _logger.LogWarning("Please change the default encryption/decryption key");

            //var configKey = configuration.GetSection("EnKey").Value;

            //Key = string.IsNullOrEmpty(configKey) ? "9a3230c9-191c-4d9d-b803-4bab3d96888a" : configKey;
        }

        public string Encrypt(string input = "")
        {
            IDataProtector protector = this._dataProtectionProvider.CreateProtector(this.Purpose);
            return protector.Protect(input);
        }

        public string Decrypt(string cipherText = "")
        {
            IDataProtector protector = this._dataProtectionProvider.CreateProtector(this.Purpose);
            return protector.Unprotect(cipherText);
        }

        public string GetMd5(string filePath)
        {
            lock (_locker)
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream stream = File.OpenRead(filePath))
                    {
                        byte[] hash = md5.ComputeHash(stream);

                        stream.Close();

                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }
    }
}