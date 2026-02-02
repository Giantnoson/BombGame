using System;
using UnityEngine;

namespace GameSystem.EventSystem
{
    public class BombEvents : MonoBehaviour
    {
        public class BombPlaceRequestEvent : GameEvent
        {
            public float BombDamage; // 炸弹伤害
            public float BombFuseTime; // 炸弹爆炸时间
            public float BombRadius; // 炸弹爆炸范围
            public Action<bool> CallBack; // 回调函数
            public Vector3 Position; // 请求放置的位置
        }

        public class BombDestroyEvent : GameEvent
        {
            public Vector3 ExplodePos; // 炸弹位置
            public Vector3 Position; // 放置位置
        }
    }
}