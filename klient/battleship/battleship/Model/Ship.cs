using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleship.Model
{
    class Ship
    {
        public Ship(int typ)
        {
            type = typ;
            coords = new int[typ];
            for (int i = 0; i < typ; i++)
                coords[i] = -1;
            coordsDone = 0;
        }

        public int type;
        public int[] coords;
        public int coordsDone;

        public bool AddCoord(int id)
        {
            if(coords[coordsDone]==-1)
            {
                coords[coordsDone] = id;
                coordsDone++;
                return true;
            }
            return false;
        }

        public bool RemoveCoord(int id)
        {
            for(int i = 0;i<type;i++)
            {
                if(id==coords[i])
                {
                    coords[i] = -1;
                    break;
                }
            }
            for(int i = 0; i<type;i++)
            {
                if(coords[i]==-1)
                {
                    if(i+1<type)
                    {
                        coords[i] = coords[i + 1];
                        coords[i + 1] = -1;

                    }

                }
                    
            }
            coordsDone--;
            return true;
        }
    }


}
