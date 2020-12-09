using System.Diagnostics;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class StaticHelpers
    {
        public static void RestartService(string serviceName, int timeoutMilliseconds)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/c net stop \"{serviceName}\" & net start \"{serviceName}\"";
            process.Start();
        }
    }
}
