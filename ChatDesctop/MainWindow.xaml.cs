
using ChatDesctop.Model;
using Hepler;
using Newtonsoft.Json;
using System.Net.Http;

using System.Threading;
using System.Windows;

using System.Windows.Input;
using System.Windows.Media;

namespace ChatDesctop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const string URL = "https://localhost:44311";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush b = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            buttonClose.Background = b;
            Close();
        }




        private async void buttonEnter_Click(object sender, RoutedEventArgs e)
        {
            string url = string.Concat(URL, "/api/authenticate/login");
            if (string.IsNullOrWhiteSpace(textBoxLogin.Text))
            {
                MessageBox.Show("Укажите логин");
                return;
            }
            if (string.IsNullOrWhiteSpace(passwordBoxPass.Password))
            {
                MessageBox.Show("Укажите пароль");
                return;
            }

            User user = new User()
            {
                Username = textBoxLogin.Text,
                Password = passwordBoxPass.Password
            };
            var body = JsonConvert.SerializeObject(user);
            HttpResponseMessage msg;
            using (msg = await Http.SendAsync(url, null, body, HttpMethod.Post, CancellationToken.None))
            {
                var response = await msg.Content.ReadAsStringAsync();
                if (!msg.IsSuccessStatusCode)
                {
                    var err = JsonConvert.DeserializeObject<Response>(response);
                    MessageBox.Show(err.Message);
                    return;
                }
                var token = JsonConvert.DeserializeObject<AccessToken>(response);
                Chat chat = new Chat(token.token, user);
                chat.Show();
                this.Close();
            }

        }

        private void ButtonRegClick(object sender, RoutedEventArgs e)
        {
            Registration a = new Registration();
            this.Close();
            a.Show();
        }

    }

}
