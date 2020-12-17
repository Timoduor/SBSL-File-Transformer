using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Infrastructure.Sftp
{
    public class SftpManager
    {
        private readonly ILogger<SftpManager> _logger;
        private readonly SftpConfig _config;
        private readonly ApplicationDbContext _dbContext;

        private readonly static object _locker = new object();

        public SftpManager(ILogger<SftpManager> logger, ApplicationDbContext dbContext, EncryptionManager encryptionManager)
        {
            _logger = logger;
            _dbContext = dbContext;

            var configurations = _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

            _config = new SftpConfig
            {
                Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                Password = encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value)
            };
        }

        public IEnumerable<SftpFile> ListAllFiles(string remoteDirectory = ".")
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);

            try
            {
                client.Connect();
                return client.ListDirectory(remoteDirectory);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public bool UploadFile(string localFilePath, string remoteFilePath)
        {
            //lock (_locker)
            {
                using (var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password))
                {
                    try
                    {
                        client.Connect();

                        var directoryPath = Path.GetDirectoryName(remoteFilePath);

                        CreateAllDirectories(client, directoryPath);

                        using (var s = File.OpenRead(localFilePath))
                        {
                            client.UploadFile(s, remoteFilePath);

                            s.Close();
                        }

                        //client.Disconnect();

                        _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteFilePath}]");
                        return true;
                    }
                    catch (Exception exception)
                    {
                        //client.Disconnect();

                        _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteFilePath}]");
                        return false;
                    }
                }
            }
        }

        public void CreateAllDirectories(SftpClient client, string path)
        {
            // Consistent forward slashes
            path = path.Replace(@"\", "/");
            foreach (string dir in path.Split('/'))
            {
                // Ignoring leading/ending/multiple slashes
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    if (!client.Exists(dir))
                    {
                        client.CreateDirectory(dir);
                    }
                    client.ChangeDirectory(dir);
                }
            }
            // Going back to default directory
            client.ChangeDirectory("/");
        }

        public void DownloadFile(string remoteFilePath, string localFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                using var s = File.Create(localFilePath);
                client.DownloadFile(remoteFilePath, s);
                _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
        }

        public void DeleteFile(string remoteFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                client.DeleteFile(remoteFilePath);
                _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
        }
    }

    public class SftpConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
