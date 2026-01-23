using UnityEngine;

namespace GameSystem.EventSystem
{
    public class CharacterEvent : GameEvent
    {
        
    }
    public class CharacterTakeDamageEvent : CharacterEvent
    {
        public string HitId; // 受伤者ID
        public float Damage; // 伤害值
    }
    
    public class CharacterDieEvent : CharacterEvent
    {
        public string AttackerID; // 攻击者ID
        public string DieId; // 死亡者ID
        public int Exp; // 获得的经验值
    }
    
    public class ExpAddEvent : CharacterEvent
    {
        public string PlayerId; // 玩家ID
        public int Exp; // 获得的经验值
    }
    
    public class LeaveUpEvent : CharacterEvent
    {
        public string PlayerId; // 玩家ID
    }
    
    public class GameOverEvent : CharacterEvent
    {
        public bool isWin; // 是否胜利
    }
    
    
}