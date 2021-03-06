﻿using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace JayBeavers.WebTraceListener
{
    public class WebTraceListener : TraceListener
    {
        private readonly bool _enabled;

        public WebTraceListener(int port = 8080)
        {
            try
            {
                WebSocketHost.Start(port);
                _enabled = true;
            }
            catch (HttpListenerException)
            {
                Trace.WriteLine("Unable to open port 8080 for listening, WebTraceListener disabled.");
            }
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
            if (_enabled)
            {
                WebSocketHost.Send(message);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (_enabled)
            {
                WebSocketHost.Send($"{eventType}:{id} - {source}");
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string message)
        {
            if (_enabled)
            {
                WebSocketHost.Send($"{eventType}:{message} - {source}");
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, params object[] args)
        {
            if (_enabled)
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                WebSocketHost.Send(args == null
                    ? $"{eventType}:{format} - {source}"
                    : $"{eventType}:{string.Format(CultureInfo.InvariantCulture, format, args)} - {source}");
            }
        }
    }
}