using ChatDesctop.Model;
using Hepler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace ChatDesctop
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        private ClientWebSocket client = null;
        private readonly Uri host = new Uri("wss://localhost:44311/chat/connect");
        private readonly User _user;

        public Chat(string token, User user)
        {
            _user = user;
            InitializeComponent();
            Connect(token);
        }

        private async Task<List<string>> GetUsers(string token)
        {
            var headers = new Dictionary<string, string>();
            headers.Add("Authorization", token);
            HttpResponseMessage msg;
            using (msg = await Http.SendAsync("https://localhost:44311/chat/connectedusers", headers, null, HttpMethod.Get, CancellationToken.None))
            {
                var response = await msg.Content.ReadAsStringAsync();
                if (!msg.IsSuccessStatusCode)
                {
                    var err = JsonConvert.DeserializeObject<Response>(response);
                    MessageBox.Show(err.Message);
                    Close();
                }
                return JsonConvert.DeserializeObject<List<string>>(response);
            }
        }


        private async void Connect(string token)
        {
            client = new ClientWebSocket();
            token = "Bearer " + token;
            client.Options.SetRequestHeader("Authorization", token);
            await client.ConnectAsync(host, CancellationToken.None);
            //Добавляем тек. юзеров
            var users = await GetUsers(token);
            foreach (var user in users)
            {
                UserBox.Items.Add(user);
            }

            await Read();
        }

        private async Task Read()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new Byte[8192]);
            while (client.State == WebSocketState.Open)
            {
                var ms = new MemoryStream();
                WebSocketReceiveResult result = await client.ReceiveAsync(buffer, CancellationToken.None);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
                ms.Seek(0, SeekOrigin.Begin);
                string content;
                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                var obj = JsonConvert.DeserializeObject<SocketMessage<string>>(content);
                if (obj != null)
                {
                    switch (obj.Type)
                    {
                        case MessageType.ChatMessage:
                            ChatBox.Items.Add(obj.Payload);
                            break;
                        case MessageType.Connect:
                            if (!UserBox.Items.Contains(obj.Payload))
                                UserBox.Items.Add(obj.Payload);
                            break;
                        case MessageType.Disconnect:
                            UserBox.Items.Remove(obj.Payload);
                            break;
                    }
                }
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var content = Message.Text;
            await SendMessage(_user.Username + ": " + content, MessageType.ChatMessage, CancellationToken.None);
            Message.Text = string.Empty;
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            UserBox.Items.Remove(_user.Username);
            await SendMessage(_user.Username, MessageType.Disconnect, CancellationToken.None);
            await client.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
            MainWindow main = new MainWindow();
            this.Close();
            main.Show();
        }

        private async Task SendMessage(string data, MessageType type, CancellationToken cancellation)
        {
            var msg = new SocketMessage<string>()
            {
                Type = type,
                Payload =  data
            };
            var json = JsonConvert.SerializeObject(msg);
            var encoded = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation);
        }
    }
}
