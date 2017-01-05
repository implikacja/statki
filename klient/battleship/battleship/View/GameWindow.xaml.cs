using battleship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace battleship.View
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private ViewModel GameModel;
        private bool _terminateGame;
        private Thread thInfotext;
        private Config conf;
        public GameWindow()
        {
            Closing += OnWindowClosing;
            _terminateGame = false;
            conf = Config.Instance;
            GameModel = new ViewModel();
            this.DataContext = GameModel;
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            thInfotext = new Thread(new ThreadStart(loop));
            thInfotext.Start();
        }

        private void loop()
        {
            while(!_terminateGame)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    GameInfo.Text = conf.InfoText;
                }));

                if(GameCore.GameFaze == 3)
                {
                    if (conf.EndGame)
                    {
                        MessageBox.Show("Twój przeciwnik zrezygnował z gry. Zamknij okno.");
                        conf.SendMessage("exit");
                    }
                    string ans = conf.GetMessage();
                    if (ans == "go")
                    {
                        GameCore.GameFaze = 2;
                        conf.InfoText = "Twój ruch";
                    }
                    else if(ans == "lose")
                    {
                        GameCore.GameFaze = 4;
                        conf.InfoText = "PRZEGRANA";
                        MessageBox.Show("PRZEGRANA");
                    }
                }
                if(conf.EndGame)
                {
                    MessageBox.Show("Twój przeciwnik zrezygnował z gry. Zamknij okno.");
                    conf.SendMessage("exit");
                }
                    Thread.Sleep(100);
            }

        }

        private void GameInfo_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _terminateGame = true;
            conf.SendMessage("exit");
        }

    }
}
