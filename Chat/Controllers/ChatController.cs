using Chat.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private static readonly Dictionary<string, WebSocket> _clients = new Dictionary<string, WebSocket>();
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Возвращает подключенных юзеров к чату
        /// </summary>
        /// <returns>Список подключенных юзеров</returns>
        [HttpGet]
        [Authorize]
        public List<string> ConnectedUsers()
        {
            return _clients.Keys.ToList();
        }

        [HttpGet]
        [Authorize]
        public async Task Connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _lock.EnterWriteLock();
                try
                {
                    _clients.Add(HttpContext.User.Identity.Name, webSocket);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                await AnnounceNewUser(HttpContext.User.Identity.Name);//Сообщаем о новом пользователе
                await WebSocketRequest(webSocket); //Слушаем вход. запросы
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }
        }
        private async Task WebSocketRequest(WebSocket webSocket)
        {
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                HandleMessage(result, buffer);

                //Отправляем всем клиентам
                foreach (var c in _clients)
                {
                    WebSocket client = _clients[c.Key];
                    try
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        RemoveClient();
                    }
                }

            }
        }
        private void HandleMessage(WebSocketReceiveResult result, ArraySegment<byte> buffer)
        {
            var ms = new MemoryStream();
            ms.Write(buffer.Array, buffer.Offset, result.Count);
            ms.Seek(0, SeekOrigin.Begin);
            string content;
            using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }
            var obj = JsonConvert.DeserializeObject<SocketMessage<string>>(content);
            if (obj.Type == MessageType.Disconnect)
                RemoveClient();
        }

        private void RemoveClient()
        {
            _lock.EnterWriteLock();
            try
            {
                _clients.Remove(HttpContext.User.Identity.Name);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private async Task SendAll(string message)
        {
            await Send(message, _clients.Values.ToArray());
        }

        private async Task AnnounceNewUser(string name)
        {
            var message = new SocketMessage<string>
            {
                Type = MessageType.Connect,
                Payload = name
            };
            string json = JsonConvert.SerializeObject(message);
            await SendAll(json);
        }


        private async Task Send(string message, params WebSocket[] socketsToSendTo)
        {
            var sockets = socketsToSendTo.Where(s => s.State == WebSocketState.Open);
            foreach (var theSocket in sockets)
            {
                var stringAsBytes = Encoding.UTF8.GetBytes(message);
                var byteArraySegment = new ArraySegment<byte>(stringAsBytes, 0, stringAsBytes.Length);
                await theSocket.SendAsync(byteArraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }



    }
}
