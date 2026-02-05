using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class PlayerControlList : ScriptableObject
    { 
        [Header("角色控制配置")]
       public List<PlayerControlConfig> playerMoveModeConfigs;

    }
    
    [Serializable]
    public class PlayerControlConfig
    {
        [Header(" 玩家控制")]
        [Tooltip("水平移动")]
        public string moveHorizontal;
        [Tooltip("垂直移动")]
        public string moveVertical;
        [Tooltip("放置炸弹")]
        public KeyCode putBomb;
        [Tooltip("移动配置描述")]
        public string description;
        ///PlayerHorizontal1
        /// PlayerVertical1
    }
}

