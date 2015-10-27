using System;
using System.Diagnostics;

namespace JayBeavers.WebTraceListener
{

    class Program
    {
        private static TraceSource _trace;

        static void Main()
        {
            _trace = new TraceSource("Default", SourceLevels.All);
            _trace.Listeners.Add(new WebTraceListener());

            var t = new System.Timers.Timer(5000);
            t.Elapsed += T_Elapsed;
            t.Start();

            Console.Read();
        }

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _trace.TraceEvent(TraceEventType.Information, 0, "Bob the fish");
            _trace.TraceEvent(TraceEventType.Error, 0, "Doesn't like water");
        }

    }
}
