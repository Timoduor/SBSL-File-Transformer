using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using X.PagedList;

namespace SbslFileTransformer.Controllers
{
    //[HandleLicense("All")]
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<LogsController> _logger;
        private readonly string _logsFolder;
        private readonly JobDisplayManager _jobManager;

        public LogsController(ILogger<LogsController> logger, ApplicationDbContext dbContext, JobDisplayManager jobManager)
        {
            this._logger = logger;
            this._dbContext = dbContext;
            this._jobManager = jobManager;

            this._logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SBSL_ETL", "logs");
        }

        public IActionResult Index(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 10;

                List<SftpUploadedFile> uploadedFiles = this._dbContext.UploadedFiles.OrderByDescending(f => f.UploadedDate)
                    .Skip((page - 1) * itemsPerPage).OrderByDescending(f => f.UploadedDate).Take(itemsPerPage).ToList();

                count = this._dbContext.UploadedFiles.Count();

                ViewBag.TotalCount = count;

                StaticPagedList<SftpUploadedFile> pagedList = new StaticPagedList<SftpUploadedFile>(uploadedFiles, page, itemsPerPage, count);

                return this.View(pagedList);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Index", "Home");
            }
        }

        public IActionResult SearchUploadedFile(string search)
        {
            List<SftpUploadedFile> uploadedFiles = this._dbContext.UploadedFiles.Where(f => f.Name.Contains(search) || f.FilePath.Contains(search) || f.Md5.Contains(search))
                    .OrderByDescending(f => f.UploadedDate).Take(200).ToList();

            return this.Json(uploadedFiles);
        }

        public IActionResult SearchVisionRecord(string search)
        {
            List<VisionRecordCollection> uploadedFiles = this._dbContext.VisionRecordCollections
                        .Where(f => f.TransDetails.Contains(search) || f.TransID.Contains(search) || f.GLTransCode.Contains(search)
                        || f.FileName.Contains(search) || f.ReferenceNumber.Contains(search) || f.CardNumber.Contains(search)
                        || f.ContractNumber.Contains(search) || f.CustomerName.Contains(search) || f.AccountNumber.Contains(search))
                    .OrderByDescending(f => f.DateExtracted).Take(500).ToList();

            return this.Json(uploadedFiles);
        }

        public IActionResult Entries(int page = 1)
        {
            try
            {
                int itemsPerPage = 10;

                IOrderedEnumerable<SqliteLog> sqliteLogs = this.GetSqliteLogs(page, itemsPerPage, out int count);

                ViewBag.TotalCount = count;

                sqliteLogs = sqliteLogs ?? new List<SqliteLog>().OrderByDescending(l => l.Id);

                StaticPagedList<SqliteLog> pagedList = new StaticPagedList<SqliteLog>(sqliteLogs, page, itemsPerPage, count);

                return this.View(pagedList);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Files(int page = 1)
        {
            try
            {
                int itemsPerPage = 10;

                IEnumerable<FileInfo> latestFiles = this.GetLogFiles(page, itemsPerPage, out int count);

                ViewBag.TotalCount = count;

                IOrderedEnumerable<FileInfo> fileInfos = latestFiles.OrderByDescending(f => f.LastWriteTime) ??
                                new List<FileInfo>().OrderByDescending(f => f.LastWriteTime);

                StaticPagedList<FileInfo> pagedList = new StaticPagedList<FileInfo>(fileInfos, page, itemsPerPage, count);

                return this.View(pagedList);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Charts()
        {
            //To show GROUPED logs, processed reports and uploaded files
            try
            {
                DateTime filesMaxDate = this._dbContext.UploadedFiles.Any() ? this._dbContext.UploadedFiles.Select(d => d.UploadedDate).Max() : DateTime.Now;

                Dictionary<string, int> filesPerDay = this._dbContext.UploadedFiles.ToList().Where(f => f.UploadedDate > filesMaxDate.AddDays(-14))
                    .GroupBy(f => f.UploadedDate.Date)
                    .ToDictionary(g => g.Key.Date.ToString("yyyy-MM-dd ddd"), g => g.Count());

                Dictionary<string, int> filesPerMonth = this._dbContext.UploadedFiles.ToList().Where(f => f.UploadedDate > filesMaxDate.AddMonths(-7))
                    .GroupBy(f => f.UploadedDate.Month)
                    .ToDictionary(g => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key), g => g.Count());

                Dictionary<string, int> logs = this.GetLast7DaysSqliteLogs(-7).GroupBy(l => l.Date.Date).OrderByDescending(g => g.Key)
                    .ToDictionary(g => g.Key.Date.ToString("yyyy-MM-dd"), g => g.Count());

                DateTime reportsMaxDate = DateTime.Now;

                if (this._dbContext.ProcessedReports.Any())
                    reportsMaxDate = this._dbContext.ProcessedReports.Select(d => d.ProcessedDate).Max();

                Dictionary<string, int> reports = this._dbContext.ProcessedReports.ToList()
                    .Where(r => r.ProcessedDate > reportsMaxDate.AddDays(-7)).GroupBy(r => r.ProcessedDate.Date)
                    .ToDictionary(g => g.Key.Date.ToString("yyyy-MM-dd"), g => g.Count());

                return this.View(new ChartObjects { UploadedFilesPerDay = filesPerDay, UploadedFilesPerMonth = filesPerMonth, Logs = logs, Reports = reports });
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }

            return this.RedirectToAction("Index", "Logs");
        }

        public IActionResult DownloadLogFile(string name)
        {
            try
            {
                string logPathFiles = Path.Combine(this._logsFolder, "log_files");

                IEnumerable<FileInfo> files = Directory.GetFiles(logPathFiles).Select(f => new FileInfo(f));

                FileInfo file = files.FirstOrDefault(f => f.Name == name);

                byte[] bytes = this.ReadAllBytes2(file.FullName);

                return this.File(bytes, "text/plain");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Files");
            }
        }

        public IActionResult CurrentJobStatuses()
        {
            IOrderedEnumerable<KeyValuePair<string, JobStatus>> jobs = this._jobManager.GetJobStatuses().OrderByDescending(j => j.Key);

            return this.Json(jobs);
        }

        public IActionResult JobStatus()
        {
            IOrderedEnumerable<KeyValuePair<string, JobStatus>> jobs = this._jobManager.GetJobStatuses().OrderByDescending(j => j.Key);

            return this.View(jobs);
        }

        public IActionResult Vision(int page = 1)
        {
            try
            {
                int count = 0;
                int itemsPerPage = 10;

                ViewBag.TotalCount = this._dbContext.VisionRecordCollections.LongCount();

                List<VisionRecordCollection> visionRecords = this._dbContext.VisionRecordCollections.OrderByDescending(f => f.DateExtracted)
                    .Skip((page - 1) * itemsPerPage).OrderByDescending(f => f.DateExtracted).Take(itemsPerPage).ToList();

                count = this._dbContext.VisionRecordCollections.Count();

                StaticPagedList<VisionRecordCollection> pagedList = new StaticPagedList<VisionRecordCollection>(visionRecords, page, itemsPerPage, count);

                return this.View(pagedList);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return this.RedirectToAction("Index", "Home");
            }
        }

        private byte[] ReadAllBytes2(string filePath, FileAccess fileAccess = FileAccess.Read,
            FileShare shareMode = FileShare.ReadWrite)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, fileAccess, shareMode))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private IEnumerable<FileInfo> GetLogFiles(int page, int itemsPerPage, out int totalCount)
        {
            string logPathFiles = Path.Combine(this._logsFolder, "log_files");

            IEnumerable<FileInfo> files = Directory.GetFiles(logPathFiles).Select(f => new FileInfo(f));

            IEnumerable<FileInfo> latestFiles = files
                .OrderByDescending(f => f.LastWriteTime).Skip(itemsPerPage * (page - 1)).Take(itemsPerPage);

            totalCount = files.Count();

            return latestFiles;
        }

        private IOrderedEnumerable<SqliteLog> GetSqliteLogs(int page, int itemsPerPage, out int totalCount)
        {
            List<SqliteLog> logs = new List<SqliteLog>();
#if DEBUG
            using (SqliteConnection connection = new SqliteConnection(@"Data Source=bin\Debug\netcoreapp3.1\sbsletl_logs.db"))
#else
            var logPathSqlite = Path.Combine(_logsFolder, "log_sqlite");
            using (var connection =
 new SqliteConnection($"Data Source={Path.Combine(logPathSqlite, "sbsletl_logs.db")}"))
#endif
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();

                command.CommandText =
                    $@"SELECT ""TimeStamp"", ""Level"", RenderedMessage, Properties, ""Exception"", id FROM Logs ORDER BY ""Timestamp"" DESC LIMIT {itemsPerPage} OFFSET {(page - 1) * itemsPerPage}";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string timestamp = reader.GetString(0);
                        string level = reader.GetString(1);
                        string renderedMessage = reader.GetString(2);
                        string properties = reader.GetString(3);
                        string exception = reader.GetString(4);

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

                SqliteCommand commandCount = connection.CreateCommand();
                commandCount.CommandText = "SELECT COUNT(*) FROM Logs";

                totalCount = Convert.ToInt32(commandCount.ExecuteScalar());
            }

            return logs.OrderByDescending(l => l.Id);
        }

        private IEnumerable<SqliteLog> GetLast7DaysSqliteLogs(int days)
        {
            List<SqliteLog> logs = new List<SqliteLog>();
#if DEBUG
            using (SqliteConnection connection = new SqliteConnection(@"Data Source=bin\Debug\netcoreapp3.1\sbsletl_logs.db"))
#else
            var logPathSqlite = Path.Combine(_logsFolder, "log_sqlite");
            using (var connection =
 new SqliteConnection($"Data Source={Path.Combine(logPathSqlite, "sbsletl_logs.db")}"))
#endif
            {
                connection.Open();

                SqliteCommand commandMaxDate = connection.CreateCommand();
                commandMaxDate.CommandText = @"SELECT ""Timestamp"" FROM Logs ORDER BY ""Timestamp"" DESC LIMIT 1";

                DateTime maxDate = DateTime.ParseExact(commandMaxDate.ExecuteScalar().ToString(), "yyyy-MM-ddTHH:mm:ss",
                    CultureInfo.InvariantCulture);

                SqliteCommand command = connection.CreateCommand();

                command.CommandText =
                    $@"select ""Timestamp"", ""Level"" from logs where ""Timestamp"" > ""{maxDate.AddDays(days).ToString("yyyy-MM-dd")}""";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string timestamp = reader.GetString(0);
                        string level = reader.GetString(1);

                        logs.Add(new SqliteLog
                        {
                            TimeStamp = timestamp,
                            Level = level,
                            Date = DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                        });
                    }
                }
            }

            return logs;
        }
    }
}