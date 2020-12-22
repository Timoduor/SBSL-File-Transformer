using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Converters
{
    public class MTFileConverter
    {
        private static object _locker = new object();

        private static (string, string[]) RenameMTFile(string originalFile, ILogger logger)
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
                logger.LogError(ex, "Error renaming file " + $"{originalFile}");
            }

            //_logger.LogInformation($"Skipping file {Path.GetFileName(originalFile)} because it does not have a sequence number");
            //send email maybe
            return (originalFile, new string[] { });
        }
    }
}
