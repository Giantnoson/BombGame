using UnityEngine;

namespace GameSystem
{
    public class BombEvents : MonoBehaviour
    {
        
    }
    
    public class BombPlaceRequestEvent : GameEvent
    {
        public Vector3 position; // 请求放置的位置
        public GameObject bombPrefab; // 炸弹预制体
        public int ownerID; // 放置者（玩家）
    }

    public class BombExplodeEvent : GameEvent
    {
        public Vector3 position; // 爆炸位置
    }
    
}