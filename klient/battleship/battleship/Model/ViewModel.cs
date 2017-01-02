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
        private Config conf = Config.Instance;

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
