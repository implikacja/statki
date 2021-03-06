﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

        public bool EndGame;
        public string Username;
        private TcpClient Client;
        private NetworkStream ns;
        public string OtherUserName;

        private Config()
        {
            Username = "";
            OtherUserName = "";
            EndGame = false;

        }
        public string GetMessage()
        {
            try
            {
                bool received = false;
                String allData = "";
                while (!received)
                {
                    Byte[] bytes = new Byte[1];
                    int k = ns.Read(bytes, 0, bytes.Length);
                    String data = Encoding.ASCII.GetString(bytes, 0, k);
                    allData += data;
                    if (allData[allData.Length - 1] == '&')
                        received = true;
                }
                allData = allData.Substring(0, allData.Length - 1);
                if (allData == "exit")
                {
                    EndGame = true;
                }
                return allData;
            }
            catch (IOException)
            {
                MessageBox.Show("Bład odbierania. Zamknij okno.");
                EndGame = true;
                return "";
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Bład odebierania. Zamknij okno.");
                EndGame = true;
                return "";
            }
            catch (SocketException)
            {
                MessageBox.Show("Bład odebierania.Rozłączono z serwerem. Zamknij okno.");
                EndGame = true;
                return "";
            }


        }

        public bool SendMessage(string txt)
        {
            try
            {
                txt = txt + "&";
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(txt);
                ns.Write(data, 0, data.Length);
                return true;
            } catch (IOException)
            {
                MessageBox.Show("Bład wysyłania. Zamknij okno.");
                EndGame = true;
                return false;
            }
            catch(ArgumentOutOfRangeException)
            {
                MessageBox.Show("Bład wysyłania. Zamknij okno.");
                EndGame = true;
                return false;
            }
            catch (SocketException)
            {
                MessageBox.Show("Bład wysyłania.Rozłączono z serwerem. Zamknij okno.");
                EndGame = true;
                return false;
            }
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
                    InfoText = "Witaj " + Username + "!";
                    InfoText = "Grasz przeciwko " + OtherUserName;
                    return true;
                }
                return false;
            }
            catch (SocketException)
            {
                return false;
            }

        }

        private string _InfoText;
        public string InfoText
        {
            get { return _InfoText; }
            set
            {
                if (value == _InfoText)
                    return;
                else
                    _InfoText = ">> " + value + "\n" + _InfoText;
            }
        }

        
    }
}
