using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace battleship.Model
{
    class Config
    {
        private static Config _instance;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Config();
                }
                return _instance;

            }
        }

        public string Username;
        private TcpClient Client;
        private NetworkStream ns;
        private int Port;
        private string Ip;
        public string OtherUserName;

        private Config()
        {
            Username = "";
            OtherUserName = "";

        }
        public string GetMessage()
        {
            int k = 0;
            Byte[] bytes = new Byte[1024];
            k = ns.Read(bytes, 0, bytes.Length);
            if (k < 0) MessageBox.Show("Bład odbierania");
            String data = Encoding.ASCII.GetString(bytes, 0, k);
            return data;
        }

        public bool SendMessage(string txt)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(txt);
            ns.Write(data, 0, data.Length);   
            return true;
        }

        public bool JoinServer(string ip, string port, string Username)
        {
            try
            {
                Client = new TcpClient(ip, Convert.ToInt32(port));
                ns = Client.GetStream();
                this.Username = Username;
                SendMessage("hi" + Username);
                string answer = GetMessage();
                if (answer.Substring(0, 2) == "hi")
                {
                    OtherUserName = answer.Substring((2));
                    MessageBox.Show(Username + OtherUserName);
                    return true;
                }
                return false;
            }
            catch (SocketException e)
            {
                return false;
            }

        }
    }
}
