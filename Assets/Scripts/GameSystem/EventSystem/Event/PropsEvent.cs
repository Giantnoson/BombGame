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
            public PropsConfig propsConfig;

            /// <summary>
            /// 离线模式构造：携带完整的 PropsStatus 引用
            /// </summary>
            public PropsStatusEnable(string ownerId, PropsStatus propsStatus)
            {
                this.ownerId = ownerId;
                this.propsStatus = propsStatus;
                this.propsConfig = propsStatus?.propsConfig;
            }

            /// <summary>
            /// 在线模式构造：仅携带 PropsConfig，无需 PropsStatus GameObject
            /// </summary>
            public PropsStatusEnable(string ownerId, PropsConfig propsConfig)
            {
                this.ownerId = ownerId;
                this.propsStatus = null;
                this.propsConfig = propsConfig;
            }
        }

        public class PropsStatusDisable : GameEvent
        {
            public string ownerId;
            public PropsStatus propsStatus;
            public PropsConfig propsConfig;

            /// <summary>
            /// 离线模式构造：携带完整的 PropsStatus 引用
            /// </summary>
            public PropsStatusDisable(string ownerId, PropsStatus propsStatus)
            {
                this.ownerId = ownerId;
                this.propsStatus = propsStatus;
                this.propsConfig = propsStatus?.propsConfig;
            }

            /// <summary>
            /// 在线模式构造：仅携带 PropsConfig，无需 PropsStatus GameObject
            /// </summary>
            public PropsStatusDisable(string ownerId, PropsConfig propsConfig)
            {
                this.ownerId = ownerId;
                this.propsStatus = null;
                this.propsConfig = propsConfig;
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