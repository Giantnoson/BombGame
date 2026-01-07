using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    public class GameEvent { }
    public class EventSystem : MonoBehaviour
    {
        public static EventSystem Instance { get; private set; }


        private void Awake()
        {
            print("EventSystem Awake");
            // 单例模式
            if (Instance == null)
            {
                print("Setting EventSystem Instance");
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        /// <summary>
        /// 事件字典：存储事件类型及其对应的处理方法
        /// 键为事件类型(Type)，值为处理该事件的委托(Action<GameEvent>)
        /// </summary>
        static readonly Dictionary<Type, Action<GameEvent>> SEvents = new Dictionary<Type, Action<GameEvent>>();

        /// <summary>
        /// 事件查找字典：用于快速查找和移除事件监听器
        /// 键为监听器委托(Delegate)，值为对应的处理方法(Action<GameEvent>)
        /// </summary>
        static readonly Dictionary<Delegate, Action<GameEvent>> SEventLookups =
            new Dictionary<Delegate, Action<GameEvent>>();

        /// <summary>
        /// 添加事件监听器：订阅指定类型的事件
        /// </summary>
        /// <typeparam name="T">事件类型，必须继承自GameEvent</typeparam>
        /// <param name="evt">事件处理方法</param>
        public static void AddListener<T>(Action<T> evt) where T : GameEvent
        {
            // 检查是否已存在该监听器，避免重复添加
            if (!SEventLookups.ContainsKey(evt))
            {
                // 创建新的处理方法，将GameEvent转换为具体事件类型T
                Action<GameEvent> newAction = (e) => evt((T) e);
                // 将监听器与处理方法关联
                SEventLookups[evt] = newAction;

                // 将处理方法添加到事件字典中
                if (SEvents.TryGetValue(typeof(T), out Action<GameEvent> internalAction))
                    // 如果事件类型已存在，则追加新的处理方法
                    SEvents[typeof(T)] = internalAction += newAction;
                else
                    // 如果事件类型不存在，则创建新的事件条目
                    SEvents[typeof(T)] = newAction;
            }
        }

        /// <summary>
        /// 移除事件监听器：取消订阅指定类型的事件
        /// </summary>
        /// <typeparam name="T">事件类型，必须继承自GameEvent</typeparam>
        /// <param name="evt">要移除的事件处理方法</param>
        public static void RemoveListener<T>(Action<T> evt) where T : GameEvent
        {
            // 查找要移除的监听器
            if (SEventLookups.TryGetValue(evt, out var action))
            {
                // 从事件字典中移除对应的处理方法
                if (SEvents.TryGetValue(typeof(T), out var tempAction))
                {
                    // 从委托链中移除处理方法
                    tempAction -= action;
                    // 如果委托链为空，则移除整个事件类型
                    if (tempAction == null)
                        SEvents.Remove(typeof(T));
                    else
                        // 否则更新委托链
                        SEvents[typeof(T)] = tempAction;
                }

                // 从查找字典中移除监听器
                SEventLookups.Remove(evt);
            }
        }

        /// <summary>
        /// 广播事件：通知所有订阅了该事件类型的监听器
        /// </summary>
        /// <param name="evt">要广播的事件对象</param>
        public static void Broadcast(GameEvent evt)
        {
            // 查找事件类型对应的处理方法并调用
            if (SEvents.TryGetValue(evt.GetType(), out var action))
                action.Invoke(evt);
        }

        /// <summary>
        /// 清空所有事件：移除所有事件监听器和事件类型
        /// 通常在场景切换或游戏重置时调用
        /// </summary>
        public static void Clear()
        {
            // 清空事件字典
            SEvents.Clear();
            // 清空查找字典
            SEventLookups.Clear();
        }
    }
}