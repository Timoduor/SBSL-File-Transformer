using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SbslFileTransformer.Infrastructure.Encryption
{
    public class EncryptionManager
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<EncryptionManager> _logger;

        private string Key = "9a3230c9-191c-4d9d-b803-4bab3d96888a";

        public EncryptionManager(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration, ILogger<EncryptionManager> logger)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;

            if(Key == "9a3230c9-191c-4d9d-b803-4bab3d96888a")
                _logger.LogWarning("Please change the default encryption/decryption key");

            var configKey = configuration.GetSection("EnKey").Value;

            Key = string.IsNullOrEmpty(configKey) ? "9a3230c9-191c-4d9d-b803-4bab3d96888a" : configKey;

        }

        public string Encrypt(string input)
        {
            var protector = _dataProtectionProvider.CreateProtector(Key);
            return protector.Protect(input);
        }

        public string Decrypt(string cipherText)
        {
            var protector = _dataProtectionProvider.CreateProtector(Key);
            return protector.Unprotect(cipherText);
        }
    }
}
