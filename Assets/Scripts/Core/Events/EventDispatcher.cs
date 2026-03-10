using System.Collections.Generic;

namespace Core.Events
{

    /// <summary>
    /// EventDispatcher 类，实现了一个事件分发器，用于管理事件的监听和触发
    /// 它支持添加、移除事件监听器，以及分发事件的功能
    /// </summary>
    public class EventDispatcher
    {

        // 定义事件处理程序的委托类型
        public delegate void OnEventHandler(Event evt);
        // 使用字典存储不同类型的事件监听器列表
        // 键为事件类型字符串，值为该类型的事件处理程序列表
        private Dictionary<string, List<OnEventHandler>> _listeners = new Dictionary<string, List<OnEventHandler>>();
        // 用于线程同步的对象
        //private object _lockListeners = new object();
        // 构造函数
        public EventDispatcher() { }
        
        /// <summary>
        /// 触发指定类型的事件，并传递事件数据
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="handler">事件处理程序</param>
        public void AddEventListener(string type, OnEventHandler handler)
        {
            //lock (_lockListeners)  // 线程同步代码块
            {
                List<OnEventHandler> list;
                // 如果字典中已存在该类型的事件监听器列表
                if (_listeners.ContainsKey(type))
                {
                    list = _listeners[type];
                }
                else
                {
                    // 如果不存在，则创建新列表并添加到字典中
                    list = new List<OnEventHandler>();
                    _listeners.Add(type, list);
                }
                // 检查处理程序是否已存在，避免重复添加
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == handler)
                    {
                        return;
                    }
                }
                // 添加处理程序到列表
                list.Add(handler);    
            }
        }

        /// <summary>
        /// 移除指定类型和处理程序的事件监听器
        /// </summary>
        /// <param name="target">要移除监听器的目标对象</param>
        public void RemoveEventListener(object target)
        {
            //lock (_lockListeners)  // 线程同步代码块
            {
                // 遍历所有事件类型
                foreach (KeyValuePair<string, List<OnEventHandler>> kv in _listeners)
                {
                    List<OnEventHandler> list = kv.Value;
                    // 从后向前遍历，以便安全地移除元素
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        // 如果处理程序的目标对象匹配，则移除
                        if (list[i].Target == target)
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
        }
        
        ///<summary>
        /// 移除指定类型和处理程序的事件监听器
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="handler">要移除的事件处理程序</param>
        public void RemoveEventListener(string type, OnEventHandler handler)
        {
            //lock (_lockListeners)  // 线程同步代码块
            {
                // 如果字典中存在该类型的事件监听器列表
                if (_listeners.ContainsKey(type))
                {
                    List<OnEventHandler> list = _listeners[type];
                    // 从后向前遍历，以便安全地移除元素
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        // 如果找到匹配的处理程序，则移除并返回
                        if (list[i] == handler)
                        {
                            list.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        /**
         * 移除所有事件监听器
         */
        public virtual void RemoveAllListener()
        {
            _listeners.Clear();
        }
        
        /// <summary>
        /// 分发事件
        /// </summary>
        /// <param name="evt">需要分发事件的对象</param>
        public virtual void DispatchEvent(Event evt)
        {
            if (evt != null)
            {
                //evt.target = this;  // 设置事件的目标对象
                //lock (_lockListeners)  // 线程同步代码块
                {
                    // 如果字典中存在该类型的事件监听器列表
                    if (_listeners.ContainsKey(evt.type))
                    {
                        List<OnEventHandler> list = _listeners[evt.type];
                        // 从后向前遍历并调用所有处理程序
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            list[i](evt);
                        }
                    }
                }
            }
        }
    }
}
