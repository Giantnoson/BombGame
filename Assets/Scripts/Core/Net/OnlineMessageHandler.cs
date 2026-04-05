using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using GameSystem.GameScene.MainMenu;
using GameSystem.GameScene.MainMenu.UI;
using GameSystem.Manager;
using GameSystem.Message;
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
                new(CmdType.Heartbeat, msg =>
                {
                    if (msg._body.GetString("result") == "success")
                    {
                        //Debug.Log("Heartbeat success");
                    }
                    else
                    {
                        Debug.Log("Heartbeat failed");
                        TcpGameClient.IsLogin = false;
                    }
                }),
                new(CmdType.Login, msg =>
                {
                    if (msg._body.GetString("result") == "success")
                    {
                        var playerId = msg._body.GetString("playerId");
                        TcpGameClient.Instance.SetConnected(playerId);
                        Debug.Log($"Login successful, playerId={playerId}");
                        TcpGameClient.IsLogin = true;
                        MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerLobbyPanel);
                    }
                    else
                    {
                        Debug.Log($"login failed: {msg._body.GetString("reason")}");
                        GlobalMessageManager.Instance.SendTopMessage($"Login failed: {msg._body.GetString("reason")}");
                    }
                }),
                new(CmdType.Exception, msg =>
                {
                    GlobalMessageManager.Instance.SendTopMessage(MessageType.System,MessageLevel.Error,msg._body.GetString("msg"));
                    
                }),
                new (CmdType.Alert, msg =>
                {
                    GlobalMessageManager.Instance.SendTopMessage(MessageType.System,MessageLevel.Warning,msg._body.GetString("msg"));
                }),
                new (CmdType.Info, msg =>
                {
                    GlobalMessageManager.Instance.SendTopMessage(MessageType.System,MessageLevel.Normal,msg._body.GetString("msg"));
                }),
                new(CmdType.EnterBaseGame, msg =>
                {
                    int mapId = msg._body.GetInt("mapId");
                    var playersInfo = msg._body.GetDictionary("playersInfo");
                    int idx = 0;
                    Debug.Log("Received match success message: mapId=" + mapId + ", playersInfo=" +
                              NetMessage.DictionaryToJsonString(playersInfo));
                    List<CharacterBaseInfo> players = new List<CharacterBaseInfo>();

                    foreach (KeyValuePair<string, object> keyValuePair in playersInfo)
                    {
                        var info = keyValuePair.Value as NetDictionary;
                        Debug.Log("Parsing player info: "+ NetMessage.DictionaryToJsonString(info));
                        string career = info.GetString("career");
                        string uname = info.GetString("uname");
                        string id = keyValuePair.Key;
                        int controlConfig = info.GetInt("controlConfig");
                        float x = info.GetInt("x") / 100f;
                        float y = info.GetInt("y") / 100f;
                        float z = info.GetInt("z") / 100f;
                        float angle = info.GetFloat("angle");
                        
                        Debug.Log("Parsed player info: career=" + career + ", uname=" + uname + ", id=" + id +
                                  ", controlConfig=" + controlConfig + ", idx=" + ", x=" + x + ", y=" + y + ", z=" + z +
                                  ", angle=" + angle);
                        if (!string.IsNullOrEmpty(career) && !string.IsNullOrEmpty(uname) && !string.IsNullOrEmpty(id))
                        {
                            PlayerControlConfig config = null;
                            if (id == TcpGameClient.PlayerId)
                            {
                                config =
                                    PlayerControlList.LoadMapSelectInfoLists(PlayerControlList.BaseConfig)[controlConfig];
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
                    }
                    // TODO: 进入游戏
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