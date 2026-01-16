using UnityEngine;

namespace GameSystem.EventSystem
{
    public class PlayerEvent : GameEvent
    {
        
    }
    public class PlayerTakeDamageEvent : PlayerEvent
    {
        public int OwnerId; // 添加创建者ID
        public int HitId; // 受伤者ID
        public float Damage; // 伤害值
    }
    
    public class PlayerDieEvent : PlayerEvent
    {
        public int AttackerID; // 攻击者ID
        public int DieId; // 死亡者ID
        public int Exp; // 获得的经验值
    }
    
    public class ExpAddEvent : PlayerEvent
    {
        public int PlayerId; // 玩家ID
        public int Exp; // 获得的经验值
    }
    
    public class LeaveUpEvent : PlayerEvent
    {
        public int PlayerId; // 玩家ID
    }
    
    public class GameOverEvent : PlayerEvent
    {
        public bool isWin; // 是否胜利
    }
    
    
}