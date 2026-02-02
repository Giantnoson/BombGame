using UnityEngine;

namespace GameSystem.EventSystem
{
    public class CharacterMoveEvent
    {
        public class UpdateSpeedEvent : GameEvent
        {
            public float Speed;

            public UpdateSpeedEvent(string id, float speed)
            {
                Id = id;
                Speed = speed;
            }
        }

        public class UpdateMoveDirectionEvent : GameEvent
        {
            public Vector3 Direction;

            public UpdateMoveDirectionEvent(string id, Vector3 direction)
            {
                Id = id;
                Direction = direction;
            }
        }

        public class UpdateRotationXEvent : GameEvent
        {
            public float MouseX;
            public float MouseY;

            public UpdateRotationXEvent(string id, float mouseX, float mouseY)
            {
                Id = id;
                MouseX = mouseX;
                MouseY = mouseY;
            }
        }
    }
}