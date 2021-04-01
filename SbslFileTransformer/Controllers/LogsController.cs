using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using X.PagedList;


namespace SbslFileTransformer.Controllers
{
    //[HandleLicense("All")]
    public class LogsController : Controller
    {
        private readonly ILogger<LogsController> _logger;
        private readonly IFileProvider _fileProvider;
        private readonly ApplicationDbContext _dbContext;
        private readonly string _logsFolder;
        public LogsController(ILogger<LogsController> logger, IFileProvider fileProvider, ApplicationDbContext dbContext)
        {
            _fileProvider = fileProvider;
            _logger = logger;
            _dbContext = dbContext;

            _logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SBSL_ETL", "logs");
        }

        public IActionResult Index(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 10;

                var uploadedFiles = _dbContext.UploadedFiles.OrderByDescending(f => f.UploadedDate).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList().OrderByDescending(f => f.UploadedDate);

                count = _dbContext.UploadedFiles.Count();

                var pagedList = new StaticPagedList<SftpUploadedFile>(uploadedFiles, page, itemsPerPage, count);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Entries(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 10;

                IOrderedEnumerable<SqliteLog> sqliteLogs = GetSqliteLogs(page, itemsPerPage, out count);

                sqliteLogs = sqliteLogs ?? new List<SqliteLog>().OrderByDescending(l => l.Id);

                var pagedList = new StaticPagedList<SqliteLog>(sqliteLogs, page, itemsPerPage, count);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Files(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 10;

                var latestFiles = GetLogFiles(page, itemsPerPage, out count);

                var fileInfos = latestFiles.OrderByDescending(f => f.LastWriteTime) ?? new List<FileInfo>().OrderByDescending(f => f.LastWriteTime);

                var pagedList = new StaticPagedList<FileInfo>(fileInfos, page, itemsPerPage, count);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Charts()
        {
            //To show GROUPED logs, processed reports and uploaded files
            try
            {


                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult DownloadLogFile(string name)
        {
            try
            {
                var logPathFiles = Path.Combine(_logsFolder, "log_files");

                var files = Directory.GetFiles(logPathFiles).Select(f => new FileInfo(f));

                var file = files.FirstOrDefault(f => f.Name == name);

                var bytes = ReadAllBytes2(file.FullName);

                return File(bytes, "text/plain");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RedirectToAction("Files");
            }
        }

        private byte[] ReadAllBytes2(string filePath, FileAccess fileAccess = FileAccess.Read, FileShare shareMode = FileShare.ReadWrite)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, fileAccess, shareMode))
            {
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }


        private IEnumerable<FileInfo> GetLogFiles(int page, int itemsPerPage, out int totalCount)
        {
            var logPathFiles = Path.Combine(_logsFolder, "log_files");

            var files = Directory.GetFiles(logPathFiles).Select(f => new FileInfo(f));

            var latestFiles = files
                      .OrderByDescending(f => f.LastWriteTime).Skip(itemsPerPage * (page - 1)).Take(itemsPerPage);

            totalCount = files.Count();

            return latestFiles;
        }

        private IOrderedEnumerable<SqliteLog> GetSqliteLogs(int page, int itemsPerPage, out int totalCount)
        {
            var logs = new List<SqliteLog>();
#if DEBUG
            using (var connection = new SqliteConnection(@"Data Source=bin\Debug\netcoreapp3.1\sbsletl_logs.db"))
#else
            var logPathSqlite = Path.Combine(_logsFolder, "log_sqlite");
            using (var connection = new SqliteConnection($"Data Source={Path.Combine(logPathSqlite, "sbsletl_logs.db")}"))
#endif
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = $@"SELECT ""TimeStamp"", ""Level"", RenderedMessage, Properties, ""Exception"", id FROM Logs ORDER BY ""Timestamp"" DESC LIMIT {itemsPerPage} OFFSET {(page - 1) * itemsPerPage}";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var timestamp = reader.GetString(0);
                        var level = reader.GetString(1);
                        var renderedMessage = reader.GetString(2);
                        var properties = reader.GetString(3);
                        var exception = reader.GetString(4);

                        logs.Add(new SqliteLog
                        {
                            TimeStamp = timestamp,
                            Level = level,
                            RenderedMessage = renderedMessage,
                            Properties = properties,
                            Exception = exception
                        });
                    }
                }

                var commandCount = connection.CreateCommand();
                commandCount.CommandText = "SELECT COUNT(*) FROM Logs";

                totalCount = Convert.ToInt32(commandCount.ExecuteScalar());
            }

            return logs.OrderByDescending(l => l.Id);
        }


    }
}
