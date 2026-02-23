using System;
using System.Collections.Generic;

namespace Core.Net
{
    public class AutoRegister : UnityEngine.MonoBehaviour
    {
        private readonly Dictionary<int, List<Action<NetMessage>>> _handlers = new();
        
        public void Register(int cmd, Action<NetMessage> handler)
        {
            if (!_handlers.ContainsKey(cmd))
            {
                _handlers[cmd] = new List<Action<NetMessage>>();
            }
            _handlers[cmd].Add(handler);

            TcpGameClient.Instance.RegisterMessageHandler(cmd, handler);
        }
        
        public void Unregister(int cmd, Action<NetMessage> handler)
        {
            if (_handlers.ContainsKey(cmd))
            {
                _handlers[cmd].Remove(handler);
                if (_handlers[cmd] == null)
                {
                    _handlers.Remove(cmd);
                }
                TcpGameClient.Instance.UnregisterMessageHandler(cmd, handler);
            }
        }
        
        void OnEnable()
        {
            foreach (var kvp in _handlers)
            {
                foreach (var handler in kvp.Value)
                {
                    TcpGameClient.Instance.RegisterMessageHandler(kvp.Key, handler);
                }
            }
        }
        
        void OnDisable()
        {
            foreach (var kvp in _handlers)
            {
                foreach (var handler in kvp.Value)
                {
                    TcpGameClient.Instance.UnregisterMessageHandler(kvp.Key, handler);
                }
            }
        }
    }
}