using System.Collections.Generic;
using System.Text;
using Config;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Map
{
    public class MapData
    {
        private readonly int _offsetDistance = 15; //偏移偏移距离（包含（0.5,0.5），到边界距离)
        public char[,] Map;

        public MapData(int offsetDistance, List<TagPointPairs> tagPointPairs)
        {
            _offsetDistance = offsetDistance;
            TagPointPairs = tagPointPairs;
            Map = new char[offsetDistance * 2, offsetDistance * 2];
        }

        public List<TagPointPairs> TagPointPairs { get; set; }

        public void PrintMap()
        {
            var mapString = new StringBuilder();
            for (var j = 0; j < _offsetDistance * 2; j++)
            {
                for (var i = 0; i < _offsetDistance * 2; i++)
                    mapString.Append(Map[i, j]);
                mapString.Append("\n");
            }

            Debug.Log(mapString);
        }
    }
}