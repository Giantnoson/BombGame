using UnityEngine;

namespace GameSystem.EventSystem.Event
{
    public class MoveEvents
    {
        public class PlayerMoveEvent : GameEvent
        {
            public string PlayerId; // 玩家ID
            public Vector3 position; // 玩家移动目标位置
            public float angle;
            
            public PlayerMoveEvent(string playerId, Vector3 position, float angle)
            {
                PlayerId = playerId;
                this.position = position;
                this.angle = angle;
            }
        }
    }
}