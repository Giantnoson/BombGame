using Config;
using GameSystem.GameProps.Item;
using UnityEngine;

namespace GameSystem.EventSystem.Event
{
    public class PropsEvent 
    {
        public class PropsStatusEnable : GameEvent
        {
            public string ownerId;
            public PropsStatus propsStatus;

            public PropsStatusEnable(string ownerId, PropsStatus propsStatus)
            {
                this.ownerId = ownerId;
                this.propsStatus = propsStatus;
            }
        }

        public class PropsStatusDisable : GameEvent
        {
            public string ownerId;
            public PropsStatus propsStatus;

            public PropsStatusDisable(string ownerId, PropsStatus propsStatus)
            {
                this.ownerId = ownerId;
                this.propsStatus = propsStatus;
            }
        }

        public class PropsCreatedEvent : GameEvent
        {
            public Vector3 Position;
            public PropsStatus PropsStatus;
        }

        public class PropsPickedUpEvent : GameEvent
        {
            public string PlayerId;
            public PropsStatus PropsStatus;
        }
    }
}