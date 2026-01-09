using UnityEngine;

namespace GameSystem.EventSystem
{
    public class PlayerEvent : MonoBehaviour
    {
        
    }
    public class PlayerTakeDamageEvent : GameEvent
    {
        public int OwnerId; // 添加创建者ID
        public int HitId; // 受伤者ID
        public float Damage; // 伤害值
    }
    
    public class PlayerDieEvent : GameEvent
    {
        public int AttackerID; // 攻击者ID
        public int DieId; // 死亡者ID
        public int Exp; // 获得的经验值
    }
    
    public class ExpAddEvent : GameEvent
    {
        public int PlayerId; // 玩家ID
        public int Exp; // 获得的经验值
    }
    
    public class LeaveUpEvent : GameEvent
    {
        public int PlayerId; // 玩家ID
    }
    
}