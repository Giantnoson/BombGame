using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using GameSystem.GameScene.MainMenu;
using GameSystem.GameScene.MainMenu.EventSystem;
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
        
        public static string PlayerId => Instance.playerId;

        private TcpClientImpl _tcp;

        private string playerId;
        private string username;
        private string password;

        private string host = "frp-any.com";
        private int port = 51493;
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
                            string playersInfo = msg.GetString("playersInfo");
                            int idx = 0;
                            Debug.Log("Received match success message: mapId=" + mapId + ", playersInfo=" + playersInfo);
                            List<CharacterBaseInfo> players = new List<CharacterBaseInfo>();
                            playersInfo.Split("|").ToList().ForEach(info =>
                            {
                                Debug.Log("Parsing player info: " + info);
                                string[] parts = info.Split(",");
                                string career = "";
                                string uname = "";
                                string id = "";
                                int controlConfig = -1;
                                float x = -1;
                                float y = -1;
                                float z = -1;
                                float angle = -1f;
                                foreach (var part in parts)
                                {
                                    string[] kv = part.Split(":");
                                    if (kv.Length == 2)
                                    {
                                        if (kv[0] == "career")
                                        {
                                            career = kv[1];
                                        }
                                        else if (kv[0] == "uname")
                                        {
                                            uname = kv[1];
                                        }
                                        else if (kv[0] == "id")
                                        {
                                            id = kv[1];
                                        }
                                        else if (kv[0] == "controlConfig")
                                        {
                                            controlConfig = int.Parse(kv[1]);
                                        }
                                        else if (kv[0] == "x")
                                        {
                                            x = int.Parse(kv[1]) / 100f;
                                        }
                                        else if (kv[0] == "y")
                                        {
                                            y = int.Parse(kv[1]) / 100f;
                                        }
                                        else if (kv[0] == "z")
                                        {
                                            z = int.Parse(kv[1]) / 100f;
                                        }
                                        else if (kv[0] == "angle")
                                        {
                                            angle = float.Parse(kv[1]);
                                        }
                                    }
                                }
                                
                                Debug.Log("Parsed player info: career=" + career + ", uname=" + uname + ", id=" + id + ", controlConfig=" + controlConfig + ", idx=" + ", x=" + x + ", y=" + y + ", z=" + z + ", angle=" + angle);

                                if (!string.IsNullOrEmpty(career) && !string.IsNullOrEmpty(uname) && !string.IsNullOrEmpty(id))
                                {
                                    PlayerControlConfig config = null;
                                    if (id != playerId)
                                    {
                                        config = PlayerControlList.LoadMapSelectInfoLists(PlayerControlList.BaseConfig)[controlConfig];
                                    }
                                    players.Add(new CharacterBaseInfo(
                                        career,
                                        uname,
                                        id,
                                        config,
                                        idx,
                                        new Vector3(x, y, z),
                                        angle
                                    ));
                                }
                                idx++;
                            });
                            Debug.Log($"Entering scene {mapId}");

                            List<MapSelectInfo> mapSelectInfoList =
                                MapSelectInfoList.LoadMapSelectInfoLists(MapSelectInfoList.OnLineConfig);
                            MapSelectInfo mapInfo = mapSelectInfoList.Find(m => m.mapId == mapId);
                            
                            if (mapInfo == null)
                            {
                                Debug.LogError($"Map info not found for mapId={mapId}");
                                return;
                            }
                            
                            GameModeSelect.Instance.SetMap(mapInfo);
                            if (GameModeSelect.Instance != null)
                            {
                                GameModeSelect.Instance.SetGameMode(GameModeType.Online, players.Count, 0);
                                GameModeSelect.CharacterBaseInfos = players;
                                GameModeSelect.Instance.StartGame();
                                MainUIManager.Instance.CloseAll();
                            }
                        }
                        else if (msg._cmd == CmdType.Move)
                        {
                            string movePlayerId = msg.GetString("id");
                            if (movePlayerId != playerId)
                            {
                                float x = msg.GetInt("x") / 100f;
                                float y = msg.GetInt("y") / 100f;
                                float z = msg.GetInt("z") / 100f;
                                float angle = msg.GetInt("angle");
                                GameEventSystem.Broadcast(new MoveEvents.PlayerMoveEvent(movePlayerId, new Vector3(x, y, z), angle));
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
