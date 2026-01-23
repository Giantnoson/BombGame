using UnityEngine;

namespace GameSystem.EventSystem
{
    public class CharacterMoveEvent
    {
        public class UpdateSpeedEvent : GameEvent
        {
            public UpdateSpeedEvent(string ownerId, float speed)
            {
                OwnerId = ownerId;
                Speed = speed;
            }

            public float Speed;
        }

        public class UpdateMoveDirectionEvent : GameEvent
        {
            public UpdateMoveDirectionEvent(string ownerId, Vector3 direction)
            {
                OwnerId = ownerId;
                Direction = direction;
            }
            public Vector3 Direction;
        }

        public class UpdateRotationXEvent : GameEvent
        {
            public UpdateRotationXEvent(string ownerId, float mouseX, float mouseY)
            {
                OwnerId = ownerId;
                MouseX = mouseX;
                MouseY = mouseY;
            }

            public float MouseX;
            public float MouseY; 

        }
        
    }
}