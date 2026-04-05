using Config;
using GameSystem.GameProps.Item;

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
    }
}