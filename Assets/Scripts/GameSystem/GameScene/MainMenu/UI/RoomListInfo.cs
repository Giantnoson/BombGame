using System.Collections.Generic;
using Core.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class RoomListInfo : MonoBehaviour
    {
        public TextMeshProUGUI roomNameText;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI hostPlayerText;
        public TextMeshProUGUI mapNameText;
        public Button joinButton;
        
        private RoomInfo roomInfo;
        
        void Start()
        {
            joinButton.onClick.AddListener(OnJoinButtonClicked);
        }
        
        void OnDestroy()
        {
            joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        }

        private void OnJoinButtonClicked()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameJoinRoom, new Dictionary<string, object>
            {
                {"roomId", roomInfo.RoomId}
            }));
        }

        public void SetRoomInfo(RoomInfo roomInfo)
        {
            this.roomInfo = roomInfo;
            // 在这里更新UI显示，例如：
            roomNameText.text = $"房间名称: {roomInfo.RoomName}";
            playerCountText.text = $"房间信息: {roomInfo.CurrentPlayers}/{roomInfo.MaxPlayers}人";
            hostPlayerText.text = $"房主名称: {roomInfo.HostPlayerName}";
            mapNameText.text = $"地图: 暂无";
        }
    }

    public class RoomInfo
    {
        public RoomInfo(string rawInfo)
        {
            foreach (var kv in rawInfo.Split("-"))
            {
                var pair = kv.Split("?");
                if (pair.Length == 2)
                {
                    info.Add(pair[0], pair[1]);
                }
            }
            
        }

        private WrappedDict info = new();
        
        public string RoomId => info.GetString("roomId");
        public string RoomName => info.GetString("roomName");
        public int CurrentPlayers => info.GetInt("memberCnt");
        public int MaxPlayers = 4; // 最大玩家数量
        public string HostPlayerName => info.GetString("leaderName");
        public string HostPlayerID => info.GetString("leaderId");
    }
}