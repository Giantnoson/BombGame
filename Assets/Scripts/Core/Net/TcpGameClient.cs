using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using GameSystem.GameScene.MainMenu;
using GameSystem.GameScene.MessageScene;
using GameSystem.UI;
using UnityEngine;

namespace Core.Net
{
    public class TcpGameClient
    {
        private static TcpGameClient _instance;
        public static TcpGameClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TcpGameClient();
                }
                return _instance;
            }
        }

        private TcpClientImpl _tcp;

        private string playerId;
        private string username;
        private string password;

        private string host = "localhost";
        private int port = 25565;
        private bool isConnected;

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
                    NetMessage msg = NetMessage.FromBytes(bytes);
                    if (msg != null)
                    {
                        if (msg._cmd == CmdType.Login)
                        {
                            if (msg.GetString("result") == "success")
                            {
                                playerId = msg.GetString("playerId");
                                isConnected = true;
                                Debug.Log($"Login successful, playerId={playerId}");
                                MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerLobbyPanel);
                            }
                            else
                            {
                                Debug.Log($"login failed: {msg.GetString("reason")}");
                            }
                        }
                        else if (msg._cmd == CmdType.Exception)
                        {
                            GlobalMessageManager.Instance.SendTopMessage(msg.GetString("msg"));
                        }
                        else if (msg._cmd == CmdType.BaseGameMatchSuccess)
                        {
                            int mapId = msg.GetInt("mapId");
                            string career = msg.GetString("career");
                            string id = msg.GetString("id");
                            string uname = msg.GetString("uname");
                            int controlConfig = msg.GetInt("controlConfig");
                            string otherPlayersInfo = msg.GetString("otherPlayers");
                            List<CharacterBaseInfo> players = new List<CharacterBaseInfo>();
                            players.Add(new CharacterBaseInfo(
                                career,
                                id,
                                uname,
                                PlayerControlList.LoadMapSelectInfoLists(PlayerControlList.BaseConfig)[controlConfig]
                            ));
                            otherPlayersInfo.Split("|").ToList().ForEach(info =>
                            {
                                string[] parts = info.Split(",");
                                string career = "";
                                string name = "";
                                string id = "";
                                foreach (var part in parts)
                                {
                                    string[] kv = part.Split(":");
                                    if (kv.Length == 2)
                                    {
                                        if (kv[0] == "career")
                                        {
                                            career = kv[1];
                                        }
                                        else if (kv[0] == "name")
                                        {
                                            name = kv[1];
                                        }
                                        else if (kv[0] == "id")
                                        {
                                            id = kv[1];
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(career) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(id))
                                {
                                    players.Add(new CharacterBaseInfo(
                                        career,
                                        name,
                                        id
                                    ));
                                }
                            });
                            Debug.Log($"Entering scene {mapId}");

                            List<MapSelectInfo> mapSelectInfoList =
                                MapSelectInfoList.LoadMapSelectInfoLists(MapSelectInfoList.BaseConfig);
                            MapSelectInfo mapInfo = mapSelectInfoList.Find(m => m.mapId == mapId);
                            
                            if (mapInfo == null)
                            {
                                Debug.LogError($"Map info not found for mapId={mapId}");
                                return;
                            }
                            
                            GameModeSelect.Instance.SetMap(mapInfo);
                            if (GameModeSelect.Instance != null)
                            {
                                GameModeSelect.Instance.SetGameMode(GameModeType.Offline, players.Count, 0);
                                GameModeSelect.CharacterBaseInfos = players;
                                GameModeSelect.Instance.StartGame();
                                MainUIManager.Instance.CloseAll();
                            }
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

        private void TryToLogin()
        {
            if (_tcp == null) { Debug.LogWarning("TCP client is null"); return; }

            var body = new Dictionary<string, object>
            {
                { "username", username },
                { "password", password }
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
