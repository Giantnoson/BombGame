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
    public class OnlineMessageHandler : MonoBehaviour
    {
        private static OnlineMessageHandler _instance;
        
        public static OnlineMessageHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("OnlineMessageHandler instance is null. Make sure it is initialized before accessing.");
                }
                return _instance;
            }
        }

        public void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterMessageHandler()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new(CmdType.Login, msg =>
                {
                    if (msg.GetString("result") == "success")
                    {
                        var playerId = msg.GetString("playerId");
                        TcpGameClient.Instance.SetConnected(playerId);
                        Debug.Log($"Login successful, playerId={playerId}");
                        MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerPlaySetPanel);
                    }
                    else
                    {
                        Debug.Log($"login failed: {msg.GetString("reason")}");
                        GlobalMessageManager.Instance.SendTopMessage($"Login failed: {msg.GetString("reason")}");
                    }
                }),
                new(CmdType.Exception,
                    msg =>
                    {
                        GlobalMessageManager.Instance.SendTopMessage($"Login failed: {msg.GetString("reason")}");
                    }),
                new(CmdType.BaseGameMatchSuccess, msg =>
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

                        Debug.Log("Parsed player info: career=" + career + ", uname=" + uname + ", id=" + id +
                                  ", controlConfig=" + controlConfig + ", idx=" + ", x=" + x + ", y=" + y + ", z=" + z +
                                  ", angle=" + angle);

                        if (!string.IsNullOrEmpty(career) && !string.IsNullOrEmpty(uname) && !string.IsNullOrEmpty(id))
                        {
                            PlayerControlConfig config = null;
                            if (id == TcpGameClient.PlayerId)
                            {
                                config =
                                    PlayerControlList.LoadMapSelectInfoLists(PlayerControlList.BaseConfig)[
                                        controlConfig];
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
                })
            });
        }
    }
}