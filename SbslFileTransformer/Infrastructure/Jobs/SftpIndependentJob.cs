using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Files;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Plugins;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using SbslFileTransformer.PluginsLocal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class SftpIndependentJob : IHostedService
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly ILogger<SftpIndependentJob> _logger;
        private ILogger<InputFileWatcher> _fileLogger;
        private readonly EncryptionManager _encryptionManager;
        private readonly EmailSender _emailSender;
        private readonly PluginManager _pluginManager;

        private readonly static object _locker = new object();

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger
            , ILogger<InputFileWatcher> fileLogger, EncryptionManager encryptionManager, EmailSender emailSender, PluginManager pluginManager)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _fileLogger = fileLogger;
            _encryptionManager = encryptionManager;
            _emailSender = emailSender;
            _pluginManager = pluginManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting Sftp Independent...");

                SftpConfigModel config;

                int prodTimeSpan = 15;
                int sbTimeSpan = 5;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

                    config = new SftpConfigModel
                    {
                        Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                        Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                        UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                        //Password = _encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value),
                        RecurseFolders = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "RecurseFolders")?.Value),
                        IncludeSandbox = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeSandbox")?.Value),
                        IncludeProduction = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value),
                        ProductionFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value,
                        SandboxFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value,
                    };

                    prodTimeSpan = Convert.ToInt32(dbContext.Configurations.FirstOrDefault(c => c.Key == "ProductionTimeSpanCheck")?.Value);
                    sbTimeSpan = Convert.ToInt32(dbContext.Configurations.FirstOrDefault(c => c.Key == "SandboxTimeSpanCheck")?.Value);
                }

                if (config.IncludeProduction)
                {
                    var fileWatcher = new InputFileWatcher(config.ProductionFolder, _fileLogger);

                    fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, true, config.ProductionFolder);

                    //sync all folders every hours
                    var timerProduction = new Timer((state) => RunFileCheckAndUpload(state, true, config.ProductionFolder).GetAwaiter().GetResult(), null, TimeSpan.Zero,
                    TimeSpan.FromMinutes(prodTimeSpan));

                }

                if (config.IncludeSandbox)
                {
                    var fileWatcher = new InputFileWatcher(config.SandboxFolder, _fileLogger);

                    fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, false, config.SandboxFolder);

                    var timerSandbox = new Timer((state) => RunFileCheckAndUpload(state, false, config.SandboxFolder).GetAwaiter().GetResult(), null, TimeSpan.Zero,
                                                    TimeSpan.FromMinutes(sbTimeSpan));

                }

                var timeValidator = new Timer((state) => RunMtSequenceValidationCheck().GetAwaiter().GetResult(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10)); //TODO


                _logger.LogInformation("Sftp Independent Job Started Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error starting SFTP independent job");
            }
        }


        private async Task RunFileCheckAndUpload(object state, bool isProduction, string productionOrSandboxFolder)
        {

            string fileToProcess = string.Empty;

            try
            {
                _logger.LogInformation($"Running file check and upload at {DateTime.Now}!");

                var path = state?.ToString();

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !File.Exists(path))
                {
                    //do check for all folders/files
                    var options = new EnumerationOptions
                    {
                        MatchCasing = MatchCasing.CaseInsensitive,
                        MatchType = MatchType.Simple,
                        RecurseSubdirectories = true
                    };

                    var files = Directory.GetFiles(productionOrSandboxFolder, "*.*", options);

                    foreach (var file in files)
                    {
                        fileToProcess = await ProcessFileAndUpload(isProduction, productionOrSandboxFolder, file);
                    }
                }
                else
                {
                    fileToProcess = await ProcessFileAndUpload(isProduction, productionOrSandboxFolder, path);
                }

                _logger.LogInformation($"File check and upload ran successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + $" Error running file check and upload {fileToProcess}");
            }
        }

        private async Task<string> ProcessFileAndUpload(bool isProduction, string productionOrSandboxFolder, string file)
        {
            var newFileName = RenameMTFile(file);

            try
            {
                var uploadCheckResult = await FileHasBeenUploadedBefore(newFileName.Item1, isProduction);

                if (uploadCheckResult.Item2)
                {
                    //_logger.LogInformation($"File {file} already uploaded!");
                    return string.Empty;
                }

                if (newFileName.Item2.Count() > 0)
                {
                    if (newFileName.Item2.Count() == 1)
                    {
                        await UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), newFileName.Item2[0], string.Empty);
                    }
                    else
                    {
                        await UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), newFileName.Item2[0], newFileName.Item2[1]);
                    }


                }
                else
                {
                    bool isBalanceFile = false;

                    if (newFileName.Item1.ToLower().Contains("Nostro_Balances_Finacle_Format".ToLower()) && Path.GetExtension(newFileName.Item1.ToLower()) != ".txt")
                    {
                        isBalanceFile = true;

                        ///var plugin = _pluginManager.GetPlugins().FirstOrDefault(p => p.Id == new Guid("701d74d6-bb48-4384-9d73-1466de46e61f"));

                        //if(plugin != null)
                        {
                            //if (await plugin.Execute(newFileName.Item1))
                            //{
                            var converter = new BalanceFileConverter();

                            if (await converter.Execute(newFileName.Item1))
                            {
                                var newPath = Path.ChangeExtension(newFileName.Item1, ".txt");

                                await UploadFileToSftp(newPath, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newPath), string.Empty, string.Empty);
                            }
                        }
                    }

                    if (!isBalanceFile)
                    {
                        await UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), string.Empty, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error uploading file");
            }

            return newFileName.Item1;
        }

        private async Task RunMtSequenceValidationCheck()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var uploadedToday = dbContext.UploadedFiles.Where(u => u.UploadedDate.Date == DateTime.Now.Date);

                    var dict = uploadedToday.GroupBy(u => u.MtStatementNo).Select(u => new { Stmt = u.Key, Max = u.Max(u => u.MtSequenceNo) }).ToList();

                    Dictionary<string, string> absent = new Dictionary<string, string>();

                    foreach (var stmt in dict)
                    {
                        var worked = int.TryParse(stmt.Max, out int result);

                        if (worked)
                        {
                            for (int i = 1; i <= Convert.ToInt32(result); i++)
                            {
                                if (uploadedToday.FirstOrDefault() == null)
                                {
                                    if (absent.ContainsKey(stmt.Stmt))
                                    {
                                        absent[stmt.Stmt] += i.ToString();
                                    }
                                    else
                                    {
                                        absent[stmt.Stmt] = i.ToString();
                                    }
                                }
                            }
                        }
                    }

                    if (absent.Where(a => !string.IsNullOrEmpty(a.Value)).Count() > 0)
                    {
                        StringBuilder message = new StringBuilder();

                        foreach (var val in absent)
                        {
                            message.AppendLine($"Statement No {val.Key} is missing Sequence Nos {val.Value}");
                        }

                        var config = await dbContext.Configurations.FirstOrDefaultAsync(c => c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

                        var recipients = config.Value.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        await _emailSender.SendMessage(recipients, "Missing Seq. Nos", message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private (string, string[]) RenameMTFile(string originalFile)
        {
            try
            {
                lock (_locker)
                {
                    if (Path.GetFileName(originalFile).Split("_").Length > 2)
                        return (originalFile, new string[] { });

                    var lines = File.ReadAllLines(originalFile);

                    var pair = lines.FirstOrDefault(l => l.Trim().StartsWith(":28C:"))?.Split(":").Last();

                    if (pair != null)
                    {
                        var toRet = pair.Split("/");

                        var stmtSeq = pair.Replace("/", "");

                        if (Path.GetFileName(originalFile).Substring(6, stmtSeq.Length) != stmtSeq)
                        {
                            var newFilename = Path.Combine(Path.GetDirectoryName(originalFile), Path.GetFileName(originalFile).Insert(6, stmtSeq));

                            if (!File.Exists(newFilename))
                            {
                                File.Copy(originalFile, newFilename);
                            }
                            //File.Delete(originalFile);

                            return (newFilename, toRet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming file " + $"{originalFile}");
            }

            //_logger.LogInformation($"Skipping file {Path.GetFileName(originalFile)} because it does not have a sequence number");
            //send email maybe
            return (originalFile, new string[] { });
        }

        private async Task UploadFileToSftp(string filePath, string md5, bool isProduction, string relativePath, string statementNo, string sequenceNo)
        {
            try
            {
                var previouslyUploaded = await FileHasBeenUploadedBefore(filePath, isProduction);

                if (previouslyUploaded.Item2)
                {
                    _logger.LogWarning($"File {filePath} has been previously uploaded. Ignoring upload");
                    return;
                }

                _logger.LogInformation($"Uploading file {filePath} to SFTP site at {DateTime.Now}!");

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var sftpManager = scope.ServiceProvider.GetService<SftpManager>();

                    string remotePath = isProduction ? "/PROD/" : "/SB/";

                    remotePath = Path.Combine(remotePath, relativePath.Replace('\\', '/'));

                    if (sftpManager.UploadFile(filePath, remotePath))
                    {
                        dbContext.UploadedFiles.Add(new SftpUploadedFile
                        {
                            FilePath = filePath,
                            IsProduction = isProduction,
                            Md5 = md5,
                            Name = Path.GetFileName(filePath),
                            Size = new FileInfo(filePath).Length,
                            UploadedDate = DateTime.Now,
                            MtStatementNo = statementNo,
                            MtSequenceNo = sequenceNo
                        });

                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation($"Uploaded file to SFTP {remotePath} site successfully!");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to upload file {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {ex.Message}" + $"{filePath}");
            }
        }

        private async Task<(string, bool)> FileHasBeenUploadedBefore(string filePath, bool isProduction)
        {
            var md5 = _encryptionManager.GetMd5(filePath);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var fileName = Path.GetFileName(filePath);

                //check if md5/filename exists
                if (await dbContext.UploadedFiles.AnyAsync(f => (f.Md5 == md5 && f.IsProduction == isProduction)
                                    || (f.Name == fileName && f.IsProduction == isProduction)))
                {
                    return (md5, true);
                }
            }
            return (md5, false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sftp Independent Job stopped");
        }
    }
}
