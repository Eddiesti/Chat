using ChatDesctop.Model;
using Hepler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows;
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
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private const string URL = "https://localhost:44311";
        public Registration()
        {
            InitializeComponent();
        }

        private void WindowMouseLeftButtonDown1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e)
        {
            SolidColorBrush b = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            buttonClose.Background = b;
            Close();
        }

        private void CancelHendler(object sender, RoutedEventArgs e)
        {
            MainWindow tmp = new MainWindow();
            Close();
            tmp.ShowDialog();
        }

        private async void RegistrationHandler(object sender, RoutedEventArgs e)
        {
            string url = string.Concat(URL, "/api/authenticate/register");
            if (string.IsNullOrWhiteSpace(textBlockLogin.Text))
            {
                MessageBox.Show("Укажите логин");
                return;
            }
            if (string.IsNullOrWhiteSpace(passwordBoxPass.Password))
            {
                MessageBox.Show("Укажите пароль");
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxEmail.Text))
            {
                MessageBox.Show("Укажите логино");
                return;
            }

            var user = new UserRegistration()
            {
                Username = textBoxLogin.Text,
                Password = passwordBoxPass.Password,
                Email = textBoxEmail.Text

            };
            var body = JsonConvert.SerializeObject(user);
            HttpResponseMessage msg;
            using (msg = await Http.SendAsync(url, null, body, HttpMethod.Post, CancellationToken.None))
            {
                if (!msg.IsSuccessStatusCode)
                {
                    var errJson = await msg.Content.ReadAsStringAsync();
                    var err = JsonConvert.DeserializeObject<Response>(errJson);
                    MessageBox.Show(err.Message);
                    return;
                }
                MainWindow mainWindow = new MainWindow();
                this.Close();
                mainWindow.Show();
            }

        }
    }
}
