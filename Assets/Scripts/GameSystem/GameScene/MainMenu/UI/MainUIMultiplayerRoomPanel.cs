using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Core.Net;
using GameSystem.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMultiplayerRoomPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerRoomPanel;
        public Button startBtn;
        public Button leaveBtn;

        public GameObject playerInfoParent;
        public GameObject playerInfoPrefab;

        private string _roomId;
        private string _roomName;
        private Dictionary<string, RoomPlayerInfo> playerInfos;
        private string _leaderId;
        
        private void Start()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new (CmdType.BaseGameCurrentRoomChange, msg =>
                {
                    var rawPlayersInfo = msg.GetString("members");
                    _leaderId = msg.GetString("leaderId");
                    _roomId = msg.GetString("roomId"); 
                    _roomName = msg.GetString("roomName");
                    Debug.Log(rawPlayersInfo);
                    foreach (var child in playerInfoParent.transform)
                    {
                        Destroy(((Transform) child).gameObject);
                    }
                    foreach (var rawInfo in rawPlayersInfo.Split("|"))
                    {
                        if (rawInfo == "") continue;
                        var info = new RoomPlayerInfo(rawInfo);
                        info.SetLeader(_leaderId);
                        
                        var obj = Instantiate(playerInfoPrefab);
                        obj.transform.SetParent(playerInfoParent.transform);
                        var playerInfo = obj.GetComponent<PlayerInfo>();
                        playerInfo.SetRoomPlayerInfo(info);
                    }
                }),
                new (CmdType.BaseGameLeaveRoom, msg =>
                {
                    MainUIManager.Instance.Back();
                })
            });
            startBtn.onClick.AddListener(OnStartClick);
            leaveBtn.onClick.AddListener(OnLeaveClick);
        }

        private void OnStartClick()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameReady));
        }

        private void OnLeaveClick()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameLeaveRoom));
        }
    }
}
