using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class OnlineConfig : ScriptableObject
    {
        private static OnlineConfig _instance;
        
        public static OnlineConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<OnlineConfig>("OnLineConfig/OnlineConfig");
                }
                return _instance;
            }
        }

        [Header("在线配置")]
        
        [Tooltip("服务器地址")]
        public string host;
        
        [Tooltip("服务器端口")]
        public int port;
        
        [Tooltip("默认玩家名称")]
        public string defaultPlayerName;
        
        [Tooltip("默认玩家密码")]
        public string defaultPlayerPassword;
        
        [Tooltip("默认玩家类型")]
        public CharacterType defaultPlayerType;
        
        [Tooltip("默认玩家控制配置ID(不大于等于4)")]
        public int defaultControllerId;
    }
}