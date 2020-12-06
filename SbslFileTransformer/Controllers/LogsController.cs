using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Models;

namespace SbslFileTransformer.Controllers
{
    public class LogsController : Controller
    {
        private readonly ILogger<LogsController> _logger;
        private readonly IFileProvider _fileProvider;
        public LogsController(ILogger<LogsController> logger, IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var files = _fileProvider.GetDirectoryContents("logs");

                var latestFiles =
                          files
                          .OrderByDescending(f => f.LastModified);

                IOrderedEnumerable<SqliteLog> sqliteLogs = await GetSqliteLogs();

                var newLogs = new LogInfo
                {
                    FileInfos = latestFiles ?? new List<IFileInfo>().OrderByDescending(f => f.LastModified),
                    SqliteLogs = sqliteLogs ?? new List<SqliteLog>().OrderByDescending(l => l.Id)
                };

                return View(newLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult DownloadLogFile(string name)
        {
            try
            {
                var files = _fileProvider.GetDirectoryContents("logs");

                var file = files.FirstOrDefault(f => f.Name == name);

                return File(file.CreateReadStream(), "text/plain");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return RedirectToAction("Logs");
            }
        }


        private async Task<IOrderedEnumerable<SqliteLog>> GetSqliteLogs()
        {
            var logs = new List<SqliteLog>();
#if DEBUG
            using (var connection = new SqliteConnection(@"Data Source=bin\Debug\netcoreapp3.1\sbsletl_logs.db"))
#else
            using (var connection = new SqliteConnection("Data Source=sbsletl_logs.db"))
#endif
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = "SELECT \"TimeStamp\", \"Level\", RenderedMessage, Properties, \"Exception\", id FROM Logs ORDER BY \"Timestamp\" DESC LIMIT 200";

                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        var timestamp = reader.GetString(0);
                        var level = reader.GetString(1);
                        var renderedMessage = reader.GetString(2);
                        var properties = reader.GetString(3);
                        var exception = reader.GetString(4);

                        logs.Add(new SqliteLog {
                            TimeStamp = timestamp,
                            Level = level,
                            RenderedMessage = renderedMessage,
                            Properties = properties,
                            Exception = exception
                        });
                    }
                }
            }

            return logs.OrderByDescending(l => l.Id);
        }
    }
}
