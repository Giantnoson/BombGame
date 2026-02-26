using System;
using System.Collections.Generic;
using Config;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Net
{
    /// <summary>
    /// TCP游戏客户端类，负责处理网络连接、消息收发和消息处理
    /// </summary>
    public class TcpGameClient
    {
        // 单例模式实例
        private static TcpGameClient _instance;
        /// <summary>
        /// 获取TcpGameClient的单例实例
        /// </summary>
        public static TcpGameClient Instance
        {
            get
            {
                _instance ??= new TcpGameClient();
                return _instance;
            }
        }
        
        /// <summary>
        /// 设置连接状态和玩家ID
        /// </summary>
        /// <param name="id">玩家ID</param>
        public void SetConnected(string id)
        {
            _playerId = id;
            _isConnected = true;
        }

        /// <summary>
        /// 获取当前玩家的ID
        /// </summary>
        public static string PlayerId => Instance._playerId;

        private TcpClientImpl _tcp; // TCP客户端实现

        private string _playerId;     // 玩家ID
        private string _username;     // 用户名
        private string _password;     // 密码

        // 从在线配置获取主机地址和端口
        private readonly string _host = OnlineConfig.Instance.host;
        private readonly int _port = OnlineConfig.Instance.port;
        private bool _isConnected;    // 连接状态标志


        /// <summary>
        /// 启动TCP连接并进行登录
        /// </summary>
        /// <param name="uname">用户名</param>
        /// <param name="pwd">密码</param>
        public void TcpStart(string uname, string pwd)
        {
            // 注册消息处理器
            OnlineMessageHandler.Instance.RegisterMessageHandler();
            // 保存用户名和密码
            _username = uname;
            _password = pwd;
            // 创建TCP客户端实例并设置回调函数
            _tcp = new TcpClientImpl
            {
                // 连接成功后的回调函数
                OnConnected = TryToLogin,
                // 连接关闭的回调函数
                OnClosed = () => Debug.LogWarning("TCP closed"),
                // 错误处理的回调函数
                OnError = (ex) => Debug.LogError($"TCP error: {ex}"),
                // 原始消息接收的处理函数
                OnMessageRaw = (bytes) =>
                {
                    try
                    {
                        // 将字节数组转换为网络消息对象
                        NetMessage msg = NetMessage.FromBytes(bytes);
                        msg.PrintLog();
                        // 如果消息不为空
                        if (msg != null)
                        {
                            // 如果不是登录消息且未连接，忽略该消息
                            if (msg._cmd != CmdType.Login && !_isConnected)
                            {
                                Debug.LogWarning($"在登录前接受消息，忽略 cmd={CmdType.TryToGetType(msg._cmd)}");
                                return;
                            }
                            // 处理消息
                            foreach (var action in _messageHandlers.GetValueOrDefault(msg._cmd,
                                         new List<Action<NetMessage>>()))
                            {
                                try
                                {
                                    // 调用消息处理函数
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

    // 连接到指定的主机和端口
            _tcp.Connect(_host, _port);
        }

        /// <summary>
        /// 注册消息处理器（批量注册）
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="handlers">默认处理器列表</param>
        public static void RegisterMessageHandler(Object obj, List<DefaultHandler> handlers)
        {
            foreach (var defaultHandler in handlers)
            {
                Instance.RegisterMessageHandler(obj, defaultHandler.CmdType, defaultHandler.Handler);
            }
        }

        /// <summary>
        /// 注册消息处理器（单个注册）
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="handler">处理函数</param>
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

        /// <summary>
        /// 注销消息处理器
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="handler">处理函数</param>
        public void UnregisterMessageHandler(Object obj, int cmdType, Action<NetMessage> handler)
        {
            var autoUnregister = obj.GetComponent<AutoRegister>();
            if (autoUnregister != null)
            {
                autoUnregister.Unregister(cmdType, handler);
            }
        }

        // 消息处理器字典，存储不同命令类型的处理函数列表
        private readonly Dictionary<int, List<Action<NetMessage>>> _messageHandlers = new();
        /// <summary>
        /// 注册消息处理器
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="handler">处理函数</param>
        public void RegisterMessageHandler(int cmdType, Action<NetMessage> handler)
        {
            if (!_messageHandlers.ContainsKey(cmdType))
            {
                _messageHandlers[cmdType] = new List<Action<NetMessage>>();
            }
            _messageHandlers[cmdType].Add(handler);
        }

        /// <summary>
        /// 注销消息处理器
        /// </summary>
        /// <param name="cmdType">命令类型</param>
        /// <param name="handler">处理函数</param>
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

        /// <summary>
        /// 尝试登录服务器
        /// </summary>
        private void TryToLogin()
        {
            if (_tcp == null) { Debug.LogWarning("TCP client is null"); return; }

            // 构建登录消息体
            var body = new Dictionary<string, object>
            {
                { "username", _username },
                { "password", _password }
            };
            var msg = new NetMessage(CmdType.Login, body);
            _tcp.SendMessage(msg);
        }

        /// <summary>
        /// 发送网络消息
        /// </summary>
        /// <param name="netMessage">要发送的网络消息</param>
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
