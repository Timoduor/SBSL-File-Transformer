using Serilog.Core;
using Serilog.Events;

namespace SbslFileTransformer.Infrastructure.SignalRLogging
{
    public class SignalRLoggerSeriLogSink : ILogEventSink
    {
        private readonly SignalRLoggingQueue _queue;

        public SignalRLoggerSeriLogSink(SignalRLoggingQueue queue) => _queue = queue;

        public void Emit(LogEvent logEvent) => _queue.Enqueue(logEvent);
    }
}
