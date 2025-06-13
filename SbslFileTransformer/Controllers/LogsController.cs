using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MySqlConnector;

using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.ViewModels;

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
        private readonly IConfiguration _configuration;

        public LogsController(ILogger<LogsController> logger, ApplicationDbContext dbContext, JobDisplayManager jobManager, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _jobManager = jobManager;
            _configuration = configuration;

            _logsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SBSL_ETL", "logs");
        }

        public IActionResult Index(int page = 1)
        {
            try
            {
                var count = 0;
                var itemsPerPage = 10;

                var uploadedFiles = _dbContext.UploadedFiles.OrderByDescending(f => f.UploadedDate)
                    .Skip((page - 1) * itemsPerPage).OrderByDescending(f => f.UploadedDate).Take(itemsPerPage).ToList();

                count = _dbContext.UploadedFiles.Count();

                ViewBag.TotalCount = count;

                var pagedList = new StaticPagedList<SftpUploadedFile>(uploadedFiles, page, itemsPerPage, count);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        public IActionResult SearchUploadedFile(string search)
        {
            var uploadedFiles = _dbContext.UploadedFiles.Where(f => f.Name.Contains(search) || f.FilePath.Contains(search) || f.Md5.Contains(search))
                    .OrderByDescending(f => f.UploadedDate).Take(200).ToList();

            return Json(uploadedFiles);
        }

        public IActionResult Entries(int page = 1)
        {
            try
            {
                var itemsPerPage = 10;

                var sqliteLogs = GetSqlLogs(page, itemsPerPage, out var count);

                ViewBag.LogLevels = new SelectList(Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>()
                    .Select(v => new SelectListItem
                    {
                        Text = v.ToString(),
                        Value = ((int)v).ToString()
                    }).ToList(), "Value", "Text");

                ViewBag.TotalCount = count;

                sqliteLogs = sqliteLogs ?? new List<LogEntries>().OrderByDescending(l => l.Id);

                var pagedList = new StaticPagedList<LogEntries>(sqliteLogs, page, itemsPerPage, count);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        public IActionResult LiveLogs()
        {
            return View();
        }

        public IActionResult Files(int page = 1)
        {
            try
            {
                var itemsPerPage = 10;

                var latestFiles = GetLogFiles(page, itemsPerPage, out var count);

                ViewBag.TotalCount = count;

                var fileInfos = latestFiles.OrderByDescending(f => f.LastWriteTime) ??
                                new List<FileInfo>().OrderByDescending(f => f.LastWriteTime);

                var pagedList = new StaticPagedList<FileInfo>(fileInfos, page, itemsPerPage, count);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        public IActionResult Charts()
        {
            //To show GROUPED logs, processed reports and uploaded files
            try
            {
                var filesMaxDate = _dbContext.UploadedFiles.Any() ? _dbContext.UploadedFiles.Select(d => d.UploadedDate).Max() : DateTime.Now;

                var filesPerDay = _dbContext.UploadedFiles.ToList().Where(f => f.UploadedDate > filesMaxDate.AddDays(-14))
                    .GroupBy(f => f.UploadedDate.Date)
                    .ToDictionary(g => g.Key.Date.ToString("yyyy-MM-dd ddd"), g => g.Count());

                var filesPerMonth = _dbContext.UploadedFiles.ToList().Where(f => f.UploadedDate > filesMaxDate.AddMonths(-7))
                    .GroupBy(f => f.UploadedDate.Month)
                    .ToDictionary(g => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key), g => g.Count());

                var logs = GetLast7DaysSqlLogs(-7).GroupBy(l => l.Date.Date).OrderByDescending(g => g.Key)
                    .ToDictionary(g => g.Key.Date.ToString("yyyy-MM-dd"), g => g.Count());

                var reportsMaxDate = DateTime.Now;

                if (_dbContext.ProcessedReports.Any())
                {
                    reportsMaxDate = _dbContext.ProcessedReports.Select(d => d.ProcessedDate).Max();
                }

                var reports = _dbContext.ProcessedReports.ToList()
                    .Where(r => r.ProcessedDate > reportsMaxDate.AddDays(-7)).GroupBy(r => r.ProcessedDate.Date)
                    .ToDictionary(g => g.Key.Date.ToString("yyyy-MM-dd"), g => g.Count());

                return View(new ChartObjects { UploadedFilesPerDay = filesPerDay, UploadedFilesPerMonth = filesPerMonth, Logs = logs, Reports = reports });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return RedirectToAction("Index", "Logs");
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

        public IActionResult CurrentJobStatuses()
        {
            var jobs = _jobManager.GetJobStatuses().OrderByDescending(j => j.Key);

            return Json(jobs);
        }

        public IActionResult JobStatus()
        {
            var jobs = _jobManager.GetJobStatuses().OrderByDescending(j => j.Key);

            return View(jobs);
        }

        public IActionResult SearchVisionRecord(string search)
        {
            var visionCollections = _dbContext.VisionRecordCollections
                .Where(f => f.TransDetails.Contains(search) || f.TransID.Contains(search) || f.GLTransCode.Contains(search)
                            || f.FileName.Contains(search) || f.ReferenceNumber.Contains(search) || f.CardNumber.Contains(search)
                            || f.ContractNumber.Contains(search) || f.CustomerName.Contains(search) || f.AccountNumber.Contains(search))
                .OrderByDescending(f => f.DateExtracted).Take(500).ToList();

            var visionSettlements = _dbContext.VisionRecordCreditSettlements
                .Where(f => f.TransDetails.Contains(search) || f.TransID.Contains(search) || f.GLTransCode.Contains(search)
                            || f.FileName.Contains(search) || f.ReferenceNumber.Contains(search) || f.CardNumber.Contains(search)
                            || f.ContractNumber.Contains(search) || f.CustomerName.Contains(search) || f.AccountNumber.Contains(search))
                .OrderByDescending(f => f.DateExtracted).Take(500).ToList();

            var visionDebtors = _dbContext.VisionRecordDebtors
                .Where(f => f.TransDetails.Contains(search) || f.TransID.Contains(search) || f.GLTransCode.Contains(search)
                            || f.FileName.Contains(search) || f.ReferenceNumber.Contains(search) || f.CardNumber.Contains(search)
                            || f.ContractNumber.Contains(search) || f.CustomerName.Contains(search) || f.AccountNumber.Contains(search))
                .OrderByDescending(f => f.DateExtracted).Take(500).ToList();

            var visionRecords =
                ((IEnumerable<VisionRecordBase>)visionCollections).Union(visionDebtors).Union(visionSettlements);

            return Json(visionRecords);
        }

        public IActionResult Vision(int page = 1)
        {
            try
            {
                var itemsPerPage = 10;

                ViewBag.TotalCount = _dbContext.VisionRecordCollections.LongCount();

                var visionRecords = _dbContext.VisionRecordCollections.OrderByDescending(f => f.DateExtracted)
                    .Skip((page - 1) * itemsPerPage).OrderByDescending(f => f.DateExtracted).Take(itemsPerPage).ToList();

                var visionRecordsSett = _dbContext.VisionRecordCreditSettlements.OrderByDescending(f => f.DateExtracted)
                    .Skip((page - 1) * itemsPerPage).OrderByDescending(f => f.DateExtracted).Take(itemsPerPage).ToList();

                var visionRecordsDebt = _dbContext.VisionRecordDebtors.OrderByDescending(f => f.DateExtracted)
                    .Skip((page - 1) * itemsPerPage).OrderByDescending(f => f.DateExtracted).Take(itemsPerPage).ToList();

                var count = _dbContext.VisionRecordCollections.Count() + _dbContext.VisionRecordCreditSettlements.Count() + _dbContext.VisionRecordDebtors.Count();

                var combinedVisionRecords = ((IEnumerable<VisionRecordBase>)visionRecords).Union(visionRecordsDebt).Union(visionRecordsSett);

                var combinedCount = combinedVisionRecords.Count();

                var pagedList = new StaticPagedList<VisionRecordBase>(combinedVisionRecords, page, itemsPerPage, combinedCount);

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Content(ex.Message);
            }
        }

        private byte[] ReadAllBytes2(string filePath, FileAccess fileAccess = FileAccess.Read,
            FileShare shareMode = FileShare.ReadWrite)
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

        private IOrderedEnumerable<LogEntries> GetSqlLogs(int page, int itemsPerPage, out int totalCount)
        {
            var logs = new List<LogEntries>();

            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText =
                    $@"SELECT TimeStamp, LogLevel, Message, Properties, Exception, id FROM Logs ORDER BY Timestamp DESC LIMIT {itemsPerPage} OFFSET {(page - 1) * itemsPerPage}";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var timestamp = reader.GetString(0);
                        var level = reader.GetString(1);
                        var renderedMessage = reader.GetValue(2)?.ToString();
                        var properties = reader.GetValue(3)?.ToString();
                        var exception = reader.GetValue(4)?.ToString();

                        logs.Add(new LogEntries
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

        private IEnumerable<LogEntries> GetLast7DaysSqlLogs(int days)
        {
            var logs = new List<LogEntries>();

            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var commandMaxDate = connection.CreateCommand();
                commandMaxDate.CommandText = @"SELECT Timestamp FROM Logs ORDER BY Timestamp DESC LIMIT 1";

                var maxDate = DateTime.Parse(commandMaxDate.ExecuteScalar().ToString());

                var command = connection.CreateCommand();

                command.CommandText =
                    $@"select Timestamp, LogLevel from logs where Timestamp > ""{maxDate.AddDays(days).ToString("yyyy-MM-dd")}""";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var timestamp = reader.GetString(0);
                        var level = reader.GetString(1);

                        logs.Add(new LogEntries
                        {
                            TimeStamp = timestamp,
                            Level = level,
                            Date = DateTime.Parse(timestamp)
                        });
                    }
                }
            }

            return logs;
        }
    }
}
