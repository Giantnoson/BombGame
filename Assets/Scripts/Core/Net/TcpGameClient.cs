using System;
using System.Collections.Generic;
using Config;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Net
{
    public class TcpGameClient
    {
        private static TcpGameClient _instance;
        public static TcpGameClient Instance
        {
            get
            {
                _instance ??= new TcpGameClient();
                return _instance;
            }
        }
        
        public void SetConnected(string id)
        {
            _playerId = id;
            _isConnected = true;
        }

        public static string PlayerId => Instance._playerId;

        private TcpClientImpl _tcp;

        private string _playerId;
        private string _username;
        private string _password;

        private readonly string _host = OnlineConfig.Instance.host;
        private readonly int _port = OnlineConfig.Instance.port;
        private bool _isConnected;

        public void TcpStart(string uname, string pwd)
        {
            OnlineMessageHandler.Instance.RegisterMessageHandler();
            _username = uname;
            _password = pwd;
            _tcp = new TcpClientImpl
            {
                OnConnected = TryToLogin,
                OnClosed = () => Debug.LogWarning("TCP closed"),
                OnError = (ex) => Debug.LogError($"TCP error: {ex}"),
                OnMessageRaw = (bytes) =>
                {
                    try
                    {
                        NetMessage msg = NetMessage.FromBytes(bytes);
                        msg.PrintLog();
                        if (msg != null)
                        {
                            if (msg._cmd != CmdType.Login && !_isConnected)
                            {
                                Debug.LogWarning($"Received message before login, ignoring. cmd={msg._cmd}");
                                return;
                            }
                            foreach (var action in _messageHandlers.GetValueOrDefault(msg._cmd, new List<Action<NetMessage>>()))
                            {
                                try
                                {
                                    action.Invoke(msg);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error in message handler for cmd={msg._cmd}: {ex}");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Message handling error: {e}");
                    }
                }
            };

            _tcp.Connect(_host, _port);
        }

        public static void RegisterMessageHandler(Object obj, List<DefaultHandler> handlers)
        {
            foreach (var defaultHandler in handlers)
            {
                Instance.RegisterMessageHandler(obj, defaultHandler.CmdType, defaultHandler.Handler);
            }
        }

        private void RegisterMessageHandler(Object obj, int cmdType, Action<NetMessage> handler)
        {
            // 在obj销毁时自动注销handler
            var autoUnregister = obj.GetComponent<AutoRegister>();
            if (autoUnregister == null)
            {
                autoUnregister = obj.AddComponent<AutoRegister>();
            }
            autoUnregister.Register(cmdType, handler);
        }

        public void UnregisterMessageHandler(Object obj, int cmdType, Action<NetMessage> handler)
        {
            var autoUnregister = obj.GetComponent<AutoRegister>();
            if (autoUnregister != null)
            {
                autoUnregister.Unregister(cmdType, handler);
            }
        }

        private readonly Dictionary<int, List<Action<NetMessage>>> _messageHandlers = new();
        public void RegisterMessageHandler(int cmdType, Action<NetMessage> handler)
        {
            if (!_messageHandlers.ContainsKey(cmdType))
            {
                _messageHandlers[cmdType] = new List<Action<NetMessage>>();
            }
            _messageHandlers[cmdType].Add(handler);
        }

        public void UnregisterMessageHandler(int cmdType, Action<NetMessage> handler)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                if (_messageHandlers.TryGetValue(cmdType, out var messageHandler))
                {
                    messageHandler.Remove(handler);
                }
            });
        }

        private void TryToLogin()
        {
            if (_tcp == null) { Debug.LogWarning("TCP client is null"); return; }

            var body = new Dictionary<string, object>
            {
                { "username", _username },
                { "password", _password }
            };
            var msg = new NetMessage(CmdType.Login, body);
            _tcp.SendMessage(msg);
        }

        public static void SendMessage(NetMessage netMessage)
        {
            if (Instance._tcp == null)
            {
                Debug.LogWarning("TCP client is null, cannot send message");
                return;
            }
            Instance._tcp.SendMessage(netMessage);
        }
    }
}
