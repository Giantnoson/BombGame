using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{

    [CreateAssetMenu()]
    public class MapSelectInfoList : ScriptableObject
    {
        public List<MapSelectInfo> mapSelectInfoList = new List<MapSelectInfo>();
    }
    
    [Serializable]
    public class MapSelectInfo
    {
        public string mapName;
        public string mapSceneName;
        public string mapDescription;
        public Sprite mapSprite;
    }
    
}