using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace battleship.Model
{
    class Field : INotifyPropertyChanged
    {

        public enum FieldType 
        {
            Unknown = 0,
            Missed = 1,
            Sinked = 2,
            Hitted = 3,
            Ship = 4,
            Builded = 5
        }

        public int id;
        public int X;
        public int Y;
        public FieldType Type;

        private string _ContentText;
        public string ContentText
        {
            get
            {
                return _ContentText;
            }
            set
            {
                _ContentText = value;
            }
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Size { get; set; }
        public bool Enabled { get; set; }

        public string _Background;
        public string Background
        {
            get
            {
                switch (Type)
                {
                    case FieldType.Unknown:
                        return "Lavender";
                    case FieldType.Missed:
                        return "DimGrey";
                    case FieldType.Hitted:
                        return "MediumOrchid";
                    case FieldType.Sinked:
                        return "DarkMagenta";
                    case FieldType.Builded:
                        return "MediumBlue";
                    case FieldType.Ship:
                        return "MidnightBlue";
                    default:
                        return "Lavender";
                }
            }
            set
            {
                if (value == "Lavender")
                    Type = FieldType.Unknown;
                else if (value == "DimGrey")
                    Type = FieldType.Missed;
                else if (value == "MediumOrchid")
                    Type = FieldType.Hitted;
                else if (value == "DarkMagenta")
                    Type = FieldType.Sinked;
                else if (value == "MediumBlue")
                    Type = FieldType.Builded;
                else if (value == "MidnightBlue")
                    Type = FieldType.Ship;
                else
                    Type = FieldType.Unknown;

                _Background = value;
                OnPropertyChanged();
            }
        }

        public Field()
        {
            SelectFieldCommand = new DelegateCommand(SetSelectedField);
        }

        public ICommand SelectFieldCommand { get; set; }
        private void SetSelectedField(object obj)
        {
            ViewModel.SelectedField = this;
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
