using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

namespace SbslFileTransformer.Hubs
{
    public class LogsHub : Hub
    {
        public async Task SendLog(string message)
        {
            await Clients.All.SendAsync("ReceiveLog", message);
        }
    }
}
