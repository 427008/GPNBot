using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using GPNBot.API.Repositories;
using GPNBot.Sec;


namespace GPNBot.API.WS
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler { get; set; }

        private readonly UsersRepository _users;

        public WebSocketManagerMiddleware(RequestDelegate next,
                                          WebSocketHandler webSocketHandler,
                                          UsersRepository users)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
            _users = users;

        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            if(context.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var token) && !string.IsNullOrEmpty(token))
            {
                var result = JWTOptions.Validate(token.ToString(), out var login, out var validTo);
                var isItLastToken = _users.IsItLastToken(login, token, validTo);
                if(result == 0 && isItLastToken)
                { 
                    context.Response.Headers["Sec-WebSocket-Protocol"] = token;
                    var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    _webSocketHandler.OnConnected(socket, login);

                    await Receive(socket, async (result, serializedMessage) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            await _webSocketHandler.ReceiveAsync(socket, result, serializedMessage).ConfigureAwait(false);
                            return;
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            try
                            {
                                await _webSocketHandler.OnDisconnected(socket);
                            }
                            catch (WebSocketException)
                            {
                                throw; //let's not swallow any exception for now
                            }
                            return;
                        }
                    });
                }
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, string> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 4]);
                string message = null;
                WebSocketReceiveResult result = null;
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            message = await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }

                    handleMessage(result, message);
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        socket.Abort();
                    }
                }
            }

            await _webSocketHandler.OnDisconnected(socket);
        }
    }
}