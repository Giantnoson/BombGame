using UnityEngine;

namespace player
{
    [CreateAssetMenu()]
    public class GlobalProper : ScriptableObject
    {
        [Tooltip("初始经验值")]
        public int initExp = 0;
        [Tooltip("初始等级")]
        public int initLevel = 1;
        [Tooltip("初始炸弹数量")]
        public int initBombCount = 0;
        [Tooltip("最大经验值")]
        public int maxExpToLevelUp = 100;
        
    }
}