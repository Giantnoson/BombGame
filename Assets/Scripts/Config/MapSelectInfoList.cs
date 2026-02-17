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
        /// <summary>
        /// 地图名称
        /// </summary>
        public string mapName;
        /// <summary>
        /// 地图描述
        /// </summary>
        public string mapSceneName;
        /// <summary>
        /// 地图描述
        /// </summary>
        public string mapDescription;
        /// <summary>
        /// 地图图片
        /// </summary>
        public Sprite mapSprite;
        /// <summary>
        /// 地图ID
        /// </summary>
        public int mapId;
    }
    
}