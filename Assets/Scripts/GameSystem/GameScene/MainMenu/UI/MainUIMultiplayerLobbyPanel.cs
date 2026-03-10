using System;
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
        public Button matchingBtn;
        public Button backBtn;
        public GameObject roomListParent;
        public GameObject roomListPrefab;
        public TMP_InputField roomNameInput;

        private void RegisterMessageHandler()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new (CmdType.BaseGameReqRoomInfo, msg =>
                {
                    var rooms = msg._body.GetDictionary("rooms");
                    foreach (Transform child in roomListParent.transform)
                    {
                        Destroy(child.gameObject);
                    }
                    if(rooms == null) return;
                    foreach (NetDictionary roomInfo in rooms.Values)
                    {
                        var info = new RoomInfo(roomInfo);
                        
                        var obj = Instantiate(roomListPrefab);
                        obj.transform.SetParent(roomListParent.transform, false);
                        var roomListInfo = obj.GetComponent<RoomListInfo>();
                        roomListInfo.SetRoomInfo(info);
                    }
                }),
                new (CmdType.BaseGameJoinRoom, msg =>
                {
                    string result = msg._body.GetString("result");
                    if (result == "success")
                    {
                        string roomInfo = msg._body.GetString("info");
                        MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerRoomPanel, parameters: new Dictionary<string, string>
                        {
                            {"info", roomInfo}
                        });
                        Hide();
                    }
                    else
                    {
                        GlobalMessageManager.Instance.SendTopMessage($"加入房间失败: {msg._body.GetString("reason")}");
                    }
                })
            });

        }


        public override void Show()
        {
            base.Show();
            RegisterMessageHandler();
            RefreshRoomList();
            createRoomBtn.onClick.AddListener(OnCreateRoomClick);
            joinRoomBtn.onClick.AddListener(OnRefreshListClick);
            backBtn.onClick.AddListener(OnBackClick);
            matchingBtn.onClick.AddListener(OnMatchingClick);
        }
        
        public override void Hide()
        {
            base.Hide();
            GetComponent<AutoRegister>()?.UnregisterAll();
        }

        private void OnCreateRoomClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerPlaySetPanel,true);
        }

        private void OnMatchingClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerRandomFitPanel,true);
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
