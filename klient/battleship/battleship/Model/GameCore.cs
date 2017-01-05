using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace battleship.Model
{
    class GameCore : INotifyPropertyChanged
    {
        private static GameCore _instance;
        public static GameCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameCore();
                }
                return _instance;

            }
        }

        public static int GameFaze;
        public int ShipsOnMap;
        public Ship[] mapShip;
        public static Config conf;

        public GameCore()
        {
            conf = Config.Instance;
            GameFaze = 1;
            ShipsOnMap = 0;
            mapShip = new Ship[10]
            {
                new Ship(4), new Ship(3), new Ship(3), new Ship(2), new Ship(2), new Ship(2), new Ship(1), new Ship(1),
                new Ship(1), new Ship(1)
            };
        }

        public bool PlaceShip(int id, ref Field[] fields)
        {
            if(ShipsOnMap<10)
            {
                
                if (FindCollisions(id, fields))
                {
                    conf.InfoText = "Błąd! Kolizja z innym statkiem.";
                    return false;
                }
                
                if(ShipIntegrity(id,fields, mapShip[ShipsOnMap].coordsDone))
                {

                    mapShip[ShipsOnMap].AddCoord(id);
                    fields[id].Background = "RoyalBlue";
                    if(mapShip[ShipsOnMap].coordsDone == mapShip[ShipsOnMap].type)
                    {
                        foreach (int item in mapShip[ShipsOnMap].coords)
                        {
                            fields[item].Background = "MidnightBlue";
                        }
                        ShipsOnMap++;
                        string txt = "";
                        for (int i = ShipsOnMap; i < 10; i++)
                        {
                            foreach (int item in mapShip[i].coords)
                            {
                                txt += "*";
                            }
                            txt += "\n";
                        }
                        conf.InfoText = "Statek nr " + ShipsOnMap + " gotowy!";
                        if (txt.Length > 0) txt = "Pozostałe statki \n" + txt;
                        conf.InfoText = txt;
                        if(ShipsOnMap<10)conf.InfoText = "Utwórz statek " + mapShip[ShipsOnMap].type + "-masztowy";

                    }
                    if (ShipsOnMap == 10)
                    {
                        SendShips(ref fields);
                    }
                    return true;
                }
                else
                {
                    conf.InfoText = "Błąd! Pole musi dotykać pozostałych";
                    return false;
                }


            }
            return false;
        }

        public bool RemoveShip(int id, ref Field[] fields)
        {
            List<int> myCoords = FindMyCoords(id, fields);
            if(myCoords.Count>1)
            {
                conf.InfoText = "Błąd! Nie można odznaczyć pola (zaburzenie integralności statku)";
                return false;
            }
            else
            {
                mapShip[ShipsOnMap].RemoveCoord(id);
                fields[id].Background = "Lavender";
                return true;

            }
        }

            List<int> FindMyCoords(int id, Field[] fields)
        {
            List<int> lista = new List<int>();
            int y = id % 10;
            int x = (id - y) / 10;

            int[] idk = new int[4];
            for (int i = 0; i < 4; i++) idk[i] = -1;
            if (x + 1 < 10)
                idk[0] = (x + 1) * 10 + y;
            if (x - 1 >= 0)
                idk[1] = (x - 1) * 10 + y;
            if (y + 1 < 10)
                idk[2] = x * 10 + (y + 1);
            if (y - 1 >= 0)
                idk[3] = x * 10 + (y - 1);
            for (int i = 0; i < 4; i++)
            {
                if (idk[i] > 99 || idk[i] < 0)
                {

                }
                else if (fields[idk[i]].Type == Field.FieldType.Builded)
                {
                    lista.Add(idk[i]);
                }
            }
            return lista;

        }

        public bool FindCollisions(int id, Field[] fields)
        {
            int y = id % 10;
            int x = (id - y) / 10;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (x + i >= 0 && x + i <= 9 && y + j >= 0 && y + j <= 9)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            int idk = ((x + i) * 10) + (y + j);
                            if (fields[idk].Type == Field.FieldType.Ship)
                                return true;

                        }
                    }
                }
            }
            return false;
        }

        public bool ShipIntegrity(int id, Field[] fields, int coordsDone)
        {//sprawdzam czy nowy maszt będzie tworzył prosty statek bez zagięć(muszą mieć takie same x lub y)
            if(coordsDone == 0)
            {
                return true;
            }
            else if(coordsDone == 1)
            {
                List<int> myCoords = FindMyCoords(id, fields);
                if (myCoords.Count == 1) return true;
            }
            else
            {
                List<int> myCoords = FindMyCoords(id, fields);
                int y = id % 10;
                int x = (id - y) / 10;
                List<int> tmpcoord = FindMyCoords(myCoords[0], fields);
                int y1 = myCoords[0] % 10;
                int x1 = (myCoords[0] - y1) / 10;
                int y2 = tmpcoord[0] % 10;
                int x2 = (tmpcoord[0] - y2) / 10;
                if(y1==y2)
                {
                    if (y != y1)
                        return false;
                }
                else if(x!=x1)
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        public void SendShips(ref Field[] fields)
        {
            //Przesłanie statków
            for (int i = 0; i < 10; i++)
            {
                string txt = "ship";
                foreach (int item in mapShip[i].coords)
                {
                    if (item < 10)
                        txt += "0" + item;
                    else
                        txt += item;

                }
                conf.SendMessage(txt);
                string answer = conf.GetMessage();
            }
            string ans = conf.GetMessage();
            if (ans == "first")
            {
                GameFaze = 2;
                conf.InfoText = "Zaczynasz!";
                conf.InfoText = "Twój ruch";
            }

            else if (ans == "second")
            {
                GameFaze = 3;
                conf.InfoText = "Przeciwnik zaczyna";
            }
            for (int i = 0; i < 100; i++)
            {
                fields[i].Background = "Lavender";
            }

        }

        public string SendQuestion(int id, ref Field[] fields)
        {
            if(id<10)
                conf.SendMessage("if" +"0"+ id);
            else conf.SendMessage("if" + id);
            string ans = conf.GetMessage();
            if(ans == "hit")
            {
                conf.InfoText = "Trafiony!";
                conf.InfoText = "Twój ruch";
            }
            else if(ans == "miss")
            {
                conf.InfoText = "Pudło!";
                conf.InfoText = "Tura przeciwnika";
            }
            else if(ans == "sink")
            {
                conf.InfoText = "Trafiony zatopiony!";
                conf.InfoText = "Twój ruch";
            }
            else if(ans == "win")
            {
                fields[id].Background = "DarkMagenta";
                MarkSinked(id, ref fields);
                GameFaze = 4;
                conf.InfoText = "WYGRANA!";
                MessageBox.Show("WYGRANA!");
            }
            return ans;
        }

        public void MarkSinked(int id, ref Field[] fields)
        {
            int y = id % 10;
            int x = (id - y) / 10;

            int[] idk = new int[4];
            for (int i = 0; i < 4; i++) idk[i] = -1;
            if (x + 1 < 10)
                idk[0] = (x + 1) * 10 + y;
            if (x - 1 >= 0)
                idk[1] = (x - 1) * 10 + y;
            if (y + 1 < 10)
                idk[2] = x * 10 + (y + 1);
            if (y - 1 >= 0)
                idk[3] = x * 10 + (y - 1);

            for (int i = 0; i < 4; i++)
            {
                if (idk[i] > 99 || idk[i] < 0)
                {

                }
                else if (fields[idk[i]].Type == Field.FieldType.Hitted)
                {
                    fields[idk[i]].Background = "DarkMagenta";
                    MarkSinked(idk[i], ref fields);
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
