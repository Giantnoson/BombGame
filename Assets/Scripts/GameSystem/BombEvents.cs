using UnityEngine;

namespace GameSystem
{
    public class BombEvents : MonoBehaviour
    {
        
    }
    
    public class BombPlaceRequestEvent : GameEvent
    {
        public Vector3 Position; // 请求放置的位置
        public GameObject BombPrefab; // 炸弹预制体
        public int OwnerId; // 放置者（玩家）
        public float BombFuseTime; // 炸弹爆炸时间
        public float BombRadius; // 炸弹爆炸范围
        public float BombDamage; // 炸弹伤害
    }

    public class BombDestroyEvent : GameEvent
    {
        public Vector3 Position; // 放置位置
        public Vector3 ExplodePos; // 炸弹位置
        public int OwnerId; // 添加创建者ID
    }
    

    
    
    
}