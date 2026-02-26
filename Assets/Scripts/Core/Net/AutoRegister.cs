using System;
using System.Collections.Generic;

namespace Core.Net
{
    /// <summary>
    /// AutoRegister类是一个Unity MonoBehaviour组件，用于自动注册和注销网络消息处理器。
    /// 它维护了一个字典来存储不同命令码对应的处理器列表，并在组件启用和禁用时自动处理注册和注销。
    /// </summary>
    public class AutoRegister : UnityEngine.MonoBehaviour
    {
        // 使用字典存储命令码与对应的处理器列表
        private readonly Dictionary<int, List<Action<NetMessage>>> _handlers = new();
        
        /// <summary>
        /// 注册网络消息处理器
        /// </summary>
        /// <param name="cmd">消息命令码</param>
        /// <param name="handler">消息处理函数</param>
        public void Register(int cmd, Action<NetMessage> handler)
        {
            // 如果字典中不包含该命令码，则创建一个新的列表
            if (!_handlers.ContainsKey(cmd))
            {
                _handlers[cmd] = new List<Action<NetMessage>>();
            }
            // 将处理器添加到对应命令码的列表中
            _handlers[cmd].Add(handler);

            TcpGameClient.Instance.RegisterMessageHandler(cmd, handler);
        }
        /// <summary>
        /// 注销网络消息处理器
        /// </summary>
        /// <param name="cmd">消息命令码</param>
        /// <param name="handler">消息处理函数</param>
        public void Unregister(int cmd, Action<NetMessage> handler)
        {
            if (_handlers.ContainsKey(cmd))
            // 如果字典中包含该命令码
            {
                _handlers[cmd].Remove(handler);
                // 从列表中移除指定的处理器
                if (_handlers[cmd] == null)
                // 如果列表为空，则从字典中移除该命令码
                {
                    _handlers.Remove(cmd);
                }
                TcpGameClient.Instance.UnregisterMessageHandler(cmd, handler);
                // 在TcpGameClient中注销消息处理器
            }
        }

        public void UnregisterAll()
        {
            foreach (var kvp in _handlers)
            {
                foreach (var handler in kvp.Value)
                {
                    TcpGameClient.Instance.UnregisterMessageHandler(kvp.Key, handler);
                }
            }
        }
        
        /// <summary>
        /// 当组件启用时调用，重新注册所有消息处理器
        /// </summary>
        void OnEnable()
        {
            foreach (var kvp in _handlers)
            // 遍历所有命令码和对应的处理器列表
            {
                foreach (var handler in kvp.Value)
                {
                    TcpGameClient.Instance.RegisterMessageHandler(kvp.Key, handler);
                    // 重新注册每个处理器
                }
            }
        }
        /// <summary>
        /// 当组件禁用时调用，注销所有消息处理器
        /// </summary>     
        void OnDisable()
        {
            UnregisterAll();
        }
    }
}