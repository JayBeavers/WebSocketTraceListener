using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTraceListener
{

    internal class WebSocketContext
    {
        internal static List<WebSocketContext> _webSockets = new List<WebSocketContext>();

        WebSocket WebSocket;
        byte[] Buffer;

        internal static void Add(Task<HttpListenerWebSocketContext> webSocket)
        {
            _webSockets.Add(new WebSocketContext(webSocket.Result.WebSocket));
        }

        internal static void Send(string message)
        {
            foreach (var ws in _webSockets)
            {
                ws.SendMessage(message);
            }
        }

        private WebSocketContext(WebSocket webSocket)
        {
            WebSocket = webSocket;
            Buffer = new byte[1024];

            // Start listening for incoming messages
            WebSocket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None).ContinueWith(ReceiveMessage);
        }

        internal void SendMessage(string s)
        {
            var b = Encoding.UTF8.GetBytes(s);
            Debug.Assert(b.Length < 256);
            WebSocket.SendAsync(new ArraySegment<byte>(b, 0, b.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        internal void ReceiveMessage(Task<WebSocketReceiveResult> taskReceiveResult)
        {
            var webSocketReceiveResult = taskReceiveResult.Result;
            if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
            {
                _webSockets.Remove(this);
                Console.WriteLine("WebSocket closed");
                WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else
            {
                Debug.Assert(webSocketReceiveResult.MessageType == WebSocketMessageType.Text);
                var s = Encoding.UTF8.GetString(Buffer, 0, webSocketReceiveResult.Count);
                Console.WriteLine(s);

                // Continue listening
                WebSocket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None).ContinueWith(ReceiveMessage);
            }
        }
    }

    class Program
    {
        static HttpListener _listener;
        static byte[] _webPage;

        static void Main(string[] args)
        {
            _webPage = Encoding.UTF8.GetBytes(File.ReadAllText("TraceLog.html"));

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
            Console.WriteLine("Listening");

            _listener.GetContextAsync().ContinueWith(Connection);

            var t = new System.Timers.Timer(5000);
            t.Elapsed += T_Elapsed;
            t.Start();


            Console.Read();
        }

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach(var ws in WebSocketContext._webSockets)
            {
                ws.SendMessage("Bob the fish");
                ws.SendMessage("Error:Doesn't like water");
            }
        }

        static void Connection(Task<HttpListenerContext> taskContext)
        {
            Console.WriteLine("Connected");

            var context = taskContext.Result;

            if (context.Request.IsWebSocketRequest)
            {
                Console.WriteLine("WebSocket Connection Started");

                var webSocketContext = context.AcceptWebSocketAsync(subProtocol: null).ContinueWith(WebSocketContext.Add);
            }
            else
            {
                using (var s = context.Response.OutputStream)
                    context.Response.OutputStream.WriteAsync(_webPage, 0, _webPage.Length);

                Console.WriteLine("Sent web page");
            }

            _listener.GetContextAsync().ContinueWith(Connection);
        }
    }
}
