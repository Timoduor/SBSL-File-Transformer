using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Infrastructure.Sftp
{
    public class SftpManager
    {
        private static readonly object _locker = new object();
        private readonly SftpConfig _config;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SftpManager> _logger;
        private readonly EmailSender _emailSender;

        public SftpManager(ILogger<SftpManager> logger, ApplicationDbContext dbContext,
            EmailSender emailSender)
        {
            _logger = logger;
            _dbContext = dbContext;
            _emailSender = emailSender;

            var configurations = _dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

            _config = new SftpConfig
            {
                Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value
            };
        }

        public IEnumerable<SftpFile> ListAllFiles(SftpClient client, string remoteDirectory = ".")
        {
            try
            {
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

        public bool UploadFile(string localFilePath, string remoteFilePath, SftpClient client)
        {
            lock (_locker)
            {
                try
                {
                    var directoryPath = Path.GetDirectoryName(remoteFilePath);

                    CreateAllDirectories(client, directoryPath);

                    using (var s = File.OpenRead(localFilePath))
                    {
                        client.UploadFile(s, remoteFilePath, true, Reponse);
                    }

                    _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteFilePath}]");
                    return true;
                }
                catch (Exception exception)
                {
                    client.Disconnect();
                    client.Dispose();

                    EmailHelpers.SendEmails(_dbContext, "Problem uploading file", $"\n\n {exception.Message}", new[] { localFilePath }, _emailSender).GetAwaiter().GetResult();

                    _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteFilePath}]");
                }
            }

            return false;
        }

        private void Reponse(ulong obj)
        {
            _logger.LogInformation($"Response from server for upload is {obj}");
        }

        private void CreateAllDirectories(SftpClient client, string path)
        {
            client.ChangeDirectory("/");

            // Consistent forward slashes
            path = path.Replace(@"\", "/");

            foreach (var dir in path.Split('/'))
                // Ignoring leading/ending/multiple slashes
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    if (!client.Exists(dir)) client.CreateDirectory(dir);
                    client.ChangeDirectory(dir);
                }

            // Going back to default directory
            client.ChangeDirectory("/");
        }

        public void DownloadFile(string remoteFilePath, string localFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName,
                _config.Password);
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
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName,
                _config.Password);
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
}