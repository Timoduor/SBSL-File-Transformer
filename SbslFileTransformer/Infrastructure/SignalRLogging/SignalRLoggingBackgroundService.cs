using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

using SbslFileTransformer.Hubs;

namespace SbslFileTransformer.Infrastructure.SignalRLogging;

public class SignalRLoggingBackgroundService : BackgroundService
{
    private readonly SignalRLoggingQueue _queue;

    private readonly IHubContext<LogsHub> _hubContext;

    public SignalRLoggingBackgroundService(SignalRLoggingQueue queue, IHubContext<LogsHub> hubContext)
    {
        _queue = queue;
        _hubContext = hubContext;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);//Give sometime for application to start before processing

        while (!stoppingToken.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out var log))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveLog", log, stoppingToken);
            }
            await Task.Delay(100, stoppingToken);
        }
    }
}
