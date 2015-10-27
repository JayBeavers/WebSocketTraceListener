using System.Diagnostics;
using System.Globalization;

namespace JayBeavers.WebTraceListener
{
    public class WebTraceListener : TraceListener
    {

        public WebTraceListener()
        {
            WebSocketHost.Start();
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
            WebSocketHost.Send(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            WebSocketHost.Send($"{eventType}:{id} - {source}");
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string message)
        {
            WebSocketHost.Send($"{eventType}:{message} - {source}");
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, params object[] args)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            WebSocketHost.Send(args == null
                ? $"{eventType}:{format} - {source}"
                : $"{eventType}:{string.Format(CultureInfo.InvariantCulture, format, args)} - {source}");
        }
    }
}