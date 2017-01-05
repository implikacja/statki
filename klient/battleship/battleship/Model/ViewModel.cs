using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace battleship.Model
{
    class ViewModel : INotifyPropertyChanged
    {
        public static GameCore game;
        public static Config conf;
        public ViewModel()
        {
            MyFields = new Field[100];
            game = GameCore.Instance;
            conf = Config.Instance;
            LoadData();
        }
        private static Field[] _Myfields;
        public Field[] MyFields
        {
            get { return _Myfields; }
            set
            {
                if (!Equals(value, _Myfields))
                {
                    _Myfields = value;
                    OnPropertyChanged();
                }
            }
        }
        


        private static Field _SelectedField;
        public static Field SelectedField
        {
            get { return _SelectedField; }
            set
            {
                _SelectedField = value;
                if (!conf.EndGame)
                {
                    switch (GameCore.GameFaze)
                    {
                        case 1:
                            //Rozmieszczanie statków
                            if (value.Type == Field.FieldType.Unknown)
                            {
                                game.PlaceShip(_SelectedField.id, ref _Myfields);
                            }
                            else if (value.Type == Field.FieldType.Builded)
                            {
                                game.RemoveShip(_SelectedField.id, ref _Myfields);
                            }
                            break;

                        case 2:
                            //Twoja tura
                            string ans = game.SendQuestion(_SelectedField.id, ref _Myfields);
                            if(ans == "hit")
                            {
                                _Myfields[_SelectedField.id].Background = "MediumOrchid";
                            }
                            else if(ans == "miss")
                            {
                                _Myfields[_SelectedField.id].Background = "Silver";
                                GameCore.GameFaze = 3;
                            }
                            else if(ans == "sink")
                            {
                                _Myfields[_SelectedField.id].Background = "DarkMagenta";
                                game.MarkSinked(_SelectedField.id, ref _Myfields);
                            }
                            break;
                        case 3:
                            //Tura przeciwnika
                            break;
                        case 4:
                            //Wyniki
                            break;

                    }
                }
            }
        }

        private void LoadData()
        {
            conf.InfoText = "Start gry!";
            const int FieldSize = 30;
            const int SpaceBetweenFields = 3;
            const int LeftSeparator = 30;

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    MyFields[i + 10 * j] = new Field()
                    {
                        id = i + 10 * j,
                        Enabled = true,
                        X = i,
                        Y = j,
                        Type = Field.FieldType.Unknown,
                        Left = LeftSeparator + ((FieldSize + SpaceBetweenFields) * i) - 210,
                        Top = 0 - (i * FieldSize + j * 8 * FieldSize + j * (FieldSize - SpaceBetweenFields)),
                        Size = FieldSize,
                        ContentText = (char)(i + 65) + (j + 1).ToString()

                    };
                }
            conf.InfoText = "Utwórz statek 4-masztowy";

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
