using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JayBeavers.WebTraceListener
{
    internal class WebSocketHost
    {
        internal static List<WebSocketHost> WebSockets = new List<WebSocketHost>();

        static HttpListener _listener;
        static byte[] _webPage;

        readonly WebSocket _webSocket;
        readonly byte[] _buffer;

        internal static void Start(int port)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("JayBeavers.WebTraceListener.TraceLog.html"))
            {
                if (stream == null)
                {
                    throw new NullReferenceException("Unable to find embedded html page");
                }

                using (var reader = new StreamReader(stream))
                {
                    _webPage = Encoding.UTF8.GetBytes(reader.ReadToEnd()); 
                }
            }            

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:" + port + "/");
            _listener.Start();

            _listener.GetContextAsync().ContinueWith(Connection);
        }

        internal static void Add(Task<HttpListenerWebSocketContext> webSocket)
        {
            WebSockets.Add(new WebSocketHost(webSocket.Result.WebSocket));
        }

        internal static void Send(string message)
        {
            foreach (var ws in WebSockets)
            {
                ws.SendMessage(message);
            }
        }

        private WebSocketHost(WebSocket webSocket)
        {
            _webSocket = webSocket;
            _buffer = new byte[1024];

            // Start listening for incoming messages
            _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None).ContinueWith(ReceiveMessage);
        }

        internal void SendMessage(string message)
        {
            var b = Encoding.UTF8.GetBytes(message);
            if (b.Length > 255)
            {
                throw new InvalidOperationException("Message too long");
            }

            _webSocket.SendAsync(new ArraySegment<byte>(b, 0, b.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        internal void ReceiveMessage(Task<WebSocketReceiveResult> taskReceiveResult)
        {
            var webSocketReceiveResult = taskReceiveResult.Result;
            if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
            {
                WebSockets.Remove(this);
                _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else
            {
                Debug.Assert(webSocketReceiveResult.MessageType == WebSocketMessageType.Text);
                var s = Encoding.UTF8.GetString(_buffer, 0, webSocketReceiveResult.Count);
                Console.WriteLine(s);

                // Continue listening
                _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None).ContinueWith(ReceiveMessage);
            }
        }

        static void Connection(Task<HttpListenerContext> taskContext)
        {
            var context = taskContext.Result;

            if (context.Request.IsWebSocketRequest)
            {
                context.AcceptWebSocketAsync(null).ContinueWith(Add);
            }
            else
            {
                using (context.Response.OutputStream)
                    context.Response.OutputStream.WriteAsync(_webPage, 0, _webPage.Length);
            }

            _listener.GetContextAsync().ContinueWith(Connection);
        }
    }
}