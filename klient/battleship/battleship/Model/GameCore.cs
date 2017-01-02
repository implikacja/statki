using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleship.Model
{
    class GameCore
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


    }
}
