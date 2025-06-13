using System.Collections.Concurrent;
using Serilog.Events;

namespace SbslFileTransformer.Infrastructure.SignalRLogging;

public class SignalRLoggingQueue
{
    private readonly ConcurrentQueue<LogEvent> _queue = new();

    public void Enqueue(LogEvent log) => _queue.Enqueue(log);

    public bool TryDequeue(out LogEvent log) => _queue.TryDequeue(out log);
}