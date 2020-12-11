using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using GPNBot.API.Tools;
using GPNBot.API.WS.Model;


namespace GPNBot.API.WS
{
    public class WebSocketHandler
    {
        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketHandler"/> class.
        /// </summary>
        /// <param name="webSocketConnectionManager">The web socket connection manager.</param>
        /// <param name="methodInvocationStrategy">The method invocation strategy used for incoming requests.</param>
        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        /// <summary>
        /// Called when a client has connected to the server.
        /// </summary>
        /// <param name="socket">The web-socket of the client.</param>
        /// <returns>Awaitable Task.</returns>
        public void OnConnected(WebSocket socket, string login)
        {
            WebSocketConnectionManager.AddSocket(socket, login);
        }

        /// <summary>
        /// Called when a client has disconnected from the server.
        /// </summary>
        /// <param name="socket">The web-socket of the client.</param>
        /// <returns>Awaitable Task.</returns>
        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetLogin(socket)).ConfigureAwait(false);
        }

        public async Task<bool> SendMessageAsync<T>(string login, WSMessage<T> message)
        {
            var socket =  WebSocketConnectionManager.GetSocketById(login);
            if (socket == null || socket.State != WebSocketState.Open)
                return false;

            var serializedMessage = JsonSerializer.Serialize(message, GPNJsonSerializer.Option()); 
            var encodedMessage = Encoding.UTF8.GetBytes(serializedMessage);
            
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: encodedMessage,
                                                                  offset: 0,
                                                                  count: encodedMessage.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None).ConfigureAwait(false);
            return true;
        }

        public async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, string receivedMessage)
        {
            // retrieve the method invocation request.

            //var invocationResult = JsonSerializer.Deserialize<InvocationResult>(receivedMessage.Data, GPNJsonSerializer.Option());
            // find the completion source in the waiting list.
            //if (_waitingRemoteInvocations.ContainsKey(invocationResult.Identifier))
            //{
                // set the result of the completion source so the invoke method continues executing.
                //_waitingRemoteInvocations[invocationResult.Identifier].SetResult(invocationResult);
                // remove the completion source from the waiting list.
                //_waitingRemoteInvocations.Remove(invocationResult.Identifier);
            //}
        }
    }
}