using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using battleship.Model;
using battleship.View;

namespace battleship
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Config conf = Config.Instance;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void JoinGame_Click(object sender, RoutedEventArgs e)
        {
            string Username = UsernameText.Text;
            if (Username.Contains("&"))
            {
                MessageBox.Show("Username nie może zawierać '&'. Twoje Username to 'Player'.");
                Username = "Player";
            }
            if (conf.JoinServer(IpText.Text, PortText.Text, Username))
            {
                GameWindow w = new GameWindow();
                w.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Nie udało się połączyć z serwerem");
                this.Close();
            }


        }


        private void EndGame_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
