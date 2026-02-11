using System;
using System.Collections.Generic;
using GameSystem.GameScene.MainMenu;
using GameSystem.GameScene.MessageScene;
using UnityEngine;

namespace Core.Net
{
    // Unity 组件包装，示例中会在 OnEnable 时连接，在 OnDisable 时断开
    public class TcpGameClient : MonoBehaviour
    {
        private static TcpGameClient _instance;
        public static TcpGameClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TcpGameClient>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TcpGameClient");
                        _instance = go.AddComponent<TcpGameClient>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private TcpClientImpl _tcp;

        private string playerId;
        private string username;
        private string password;

        public string host = "frp-any.com";
        public int port = 51493;
        public bool isConnected;

        public void TcpStart(string uname, string pwd)
        {
            username = uname;
            password = pwd;
            _tcp = new TcpClientImpl();
            _tcp.OnConnected = () => TryToLogin();
            _tcp.OnClosed = () => Debug.LogWarning("TCP closed");
            _tcp.OnError = (ex) => Debug.LogError($"TCP error: {ex}");
            _tcp.OnMessageRaw = (bytes) =>
            {
                try
                {
                    Message msg = Message.FromBytes(bytes);
                    if (msg != null)
                    {
                        if (msg._cmd == CmdType.Login)
                        {
                            if ((string)msg._body["result"] == "success")
                            {
                                playerId = msg._body["playerId"].ToString();
                                isConnected = true;
                                Debug.Log($"Login successful, playerId={playerId}");
                                MainUIManager.Instance.ShowPanel("MultiplayerLobbyPanel");
                            }
                            else
                            {
                                Debug.Log($"login failed: {msg._body["reason"]}");
                            }
                        }
                        else if (msg._cmd == CmdType.Exception)
                        {
                            GlobalMessageManager.Instance.SendTopMessage(msg._body["msg"].ToString());
                        }
                        else
                        {
                            if (!isConnected)
                            {
                                Debug.Log("Received message before login, ignoring. cmd=" + msg._cmd);
                            }
                            else
                            {
                                Debug.Log($"Received message cmd={msg._cmd}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Message handling error: {e}");
                }
            };

            _tcp.Connect(host, port);
        }

        void ReLogin()
        {
            if (username == null || password == null)
            {
                Debug.LogWarning("Username or password is null, cannot re-login");
                return;
            }
            isConnected = false;
            Debug.Log("Re-attempting login via TCP");
            System.Threading.Thread.Sleep(1000);
            TryToLogin();
        }

        public void TryToLogin()
        {
            if (_tcp == null) { Debug.LogWarning("TCP client is null"); return; }

            var body = new Dictionary<string, object>
            {
                { "username", username },
                { "password", password }
            };
            var msg = new Message(CmdType.Login, body);
            try
            {
                _tcp.SendMessage(msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"Send login failed: {e}");
            }
        }
    }
}
