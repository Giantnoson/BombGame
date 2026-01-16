using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameSystem.Map
{
    public class MapData
    {
        int _offsetDistance = 15;//偏移偏移距离（包含（0.5,0.5），到边界距离)
        public char[,] Map;
        public List<TagPointPairs> TagPointPairs { get; set; }

        public MapData(int offsetDistance, List<TagPointPairs> tagPointPairs)
        {
            this._offsetDistance = offsetDistance;
            TagPointPairs = tagPointPairs;
            Map = new char[offsetDistance * 2, offsetDistance * 2];
        }

        public void PrintMap()
        {
            StringBuilder mapString = new StringBuilder();
            for (int j = 0; j < _offsetDistance * 2; j++)
            {
                for (int i = 0; i < _offsetDistance * 2; i++)
                    mapString.Append(Map[i, j]) ;
                mapString.Append("\n");
            }
            Debug.Log(mapString);
        }
    }
}