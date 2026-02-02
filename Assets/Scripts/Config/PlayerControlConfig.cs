using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class PlayerControlConfig : ScriptableObject
    {
        [Header(" 玩家控制")]
        [Tooltip("水平移动")]
        public string moveHorizontal;
        [Tooltip("垂直移动")]
        public string moveVertical;
        [Tooltip("放置炸弹")]
        public KeyCode putBomb;
    }
}