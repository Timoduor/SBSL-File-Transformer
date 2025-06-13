using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

using SbslFileTransformer.Hubs;

using Serilog.Events;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            ///added for debugging puoses, you can remove this line later
            await _hubContext.Clients.All.SendAsync("ReceiveLog", new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, null, MessageTemplate.Empty, Enumerable.Empty<LogEventProperty>()), stoppingToken);

            while (_queue.TryDequeue(out var log))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveLog", log, stoppingToken);
            }
            await Task.Delay(100, stoppingToken);
        }
    }
}
