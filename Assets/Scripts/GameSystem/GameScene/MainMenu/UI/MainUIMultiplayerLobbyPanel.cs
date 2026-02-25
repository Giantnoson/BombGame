using System.Collections.Generic;
using Core.Net;
using GameSystem.GameScene.MessageScene;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMultiplayerLobbyPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerLobbyPanel;
        public Button createRoomBtn;
        public Button joinRoomBtn; // Generic join or refresh
        /*
        public GameObject roomEntryPrefab; // Simple prefab with a button
        */
        public Button backBtn;
        public GameObject roomListParent;
        public GameObject roomListPrefab;
        public TMP_InputField roomNameInput;

        private void Start()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new (CmdType.BaseGameReqRoomInfo, msg =>
                {
                    var rooms = msg.GetString("rooms");
                    foreach (Transform child in roomListParent.transform)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach (var roomInfo in rooms.Split("%"))
                    {
                        if (roomInfo == "") continue;
                        var info = new RoomInfo(roomInfo);
                        
                        var obj = Instantiate(roomListPrefab);
                        obj.transform.SetParent(roomListParent.transform, false);
                        var roomListInfo = obj.GetComponent<RoomListInfo>();
                        roomListInfo.SetRoomInfo(info);
                    }
                }),
                new (CmdType.BaseGameJoinRoom, msg =>
                {
                    string result = msg.GetString("result");
                    if (result == "success")
                    {
                        string roomInfo = msg.GetString("info");
                        MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerRoomPanel, parameters: new Dictionary<string, string>
                        {
                            {"info", roomInfo}
                        });
                    }
                    else
                    {
                        GlobalMessageManager.Instance.SendTopMessage($"加入房间失败: {msg.GetString("reason")}");
                    }
                })
            });
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameReqRoomInfo));
            createRoomBtn.onClick.AddListener(OnCreateRoomClick);
            joinRoomBtn.onClick.AddListener(OnRefreshListClick);
            backBtn.onClick.AddListener(OnBackClick);
        }

        public override void Show()
        {
            base.Show();
            RefreshRoomList();
        }

        private void OnCreateRoomClick()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameCreateRoom, new Dictionary<string, object>
            {
                {"roomName", roomNameInput.text}
            }));
        }

        private void OnRefreshListClick()
        {
            RefreshRoomList();
        }

        private void RefreshRoomList()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameReqRoomInfo));
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
