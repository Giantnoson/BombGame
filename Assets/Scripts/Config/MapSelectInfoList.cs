using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{

    [CreateAssetMenu()]
    public class MapSelectInfoList : ScriptableObject
    {
        public List<MapSelectInfo> mapSelectInfoList = new List<MapSelectInfo>();
        
        public static string BaseConfig = "BaseConfig/MapSelectInfoList";
        public static string OnLineConfig = "OnLineConfig/MapSelectInfoList";
        
        public static List<MapSelectInfo> LoadMapSelectInfoLists(string path)
        {
            return Resources.Load<MapSelectInfoList>(path).mapSelectInfoList;
        }
        
    }
    
    [Serializable]
    public class MapSelectInfo
    {
        public string mapName;
        public string mapSceneName;
        public string mapDescription;
        public Sprite mapSprite;
        public int mapId;
    }
    
}