using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Net
{
    // Unity 组件包装，示例中会在 OnEnable 时连接，在 OnDisable 时断开
    public class TcpClientBehaviour : MonoBehaviour
    {
        private TcpClientImpl _tcp;

        public string host = "localhost";
        public int port = 9999;
        public bool isConnected = false;

        void OnEnable()
        {
            _tcp = new TcpClientImpl();
            _tcp.OnConnected = () =>
            {
                TryToLogin();
            };
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
                                isConnected = true;
                            }
                            else
                            {
                                ReLogin();
                            }
                        }
                        else
                        {
                            if (!isConnected)
                            {
                                ReLogin();
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
            isConnected = false;
            Debug.Log("Re-attempting login via TCP");
            System.Threading.Thread.Sleep(1000);
            TryToLogin();
        }

        void OnDisable()
        {
            _tcp?.Close();
            _tcp = null;
        }

        public void TryToLogin()
        {
            if (_tcp == null) { Debug.LogWarning("TCP client is null"); return; }

            var body = new Dictionary<string, object> { { "playerId", "111234" } };
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
