using System.Collections.Generic;
using Core.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class PlayerInfo : MonoBehaviour
    {
        [Header("UI界面")]
        [Tooltip("玩家名")]
        public TextMeshProUGUI playerNameText;
        [Tooltip("玩家类型")]
        public TextMeshProUGUI playerTypeText;
        [Tooltip("玩家状态")]
        public TextMeshProUGUI playerStatusText;

        public Transform playerBtnTran;
        public Button removePlayer;
        public Button transferRoomOwner;

        RoomPlayerInfo roomPlayerInfo;

        public void SetRoomPlayerInfo(RoomPlayerInfo roomPlayerInfo)
        {
            this.roomPlayerInfo = roomPlayerInfo;
            playerNameText.text = roomPlayerInfo.UName;
            playerTypeText.text = roomPlayerInfo.Carrer;
            playerStatusText.text = roomPlayerInfo.IsReady ? "准备" : "未准备";
            bool active = roomPlayerInfo.PlayerId != TcpGameClient.PlayerId;
            active = active && TcpGameClient.PlayerId == roomPlayerInfo.LeaderId;
            removePlayer.gameObject.SetActive(active);
            transferRoomOwner.gameObject.SetActive(active);
        }

        void Start()
        {
            removePlayer.onClick.AddListener(OnRemovePlayer);
            transferRoomOwner.onClick.AddListener(OnTransferRoomOwner);
        }
        
        void OnDestroy()
        {
            removePlayer.onClick.RemoveListener(OnRemovePlayer);
            transferRoomOwner.onClick.RemoveListener(OnTransferRoomOwner);
        }

        private void OnRemovePlayer()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameKickPlayer, new Dictionary<string, object>
            {
                {"targetId", roomPlayerInfo.PlayerId},
            }));
        }

        private void OnTransferRoomOwner()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameLeaderChange, new Dictionary<string, object>
            {
                {"targetId", roomPlayerInfo.PlayerId},
            }));
        }
    }
    
    public class RoomPlayerInfo
    {
        public RoomPlayerInfo(string rawInfo)
        {
            foreach (var kv in rawInfo.Split(","))
            {
                var pair = kv.Split(":");
                if (pair.Length == 2)
                {
                    info.Add(pair[0], pair[1]);
                }
            }
        }
        
        private WrappedDict info = new();

        public string UName => info.GetString("uname");
        public string PlayerId => info.GetString("id");
        public string Carrer => info.GetString("career");
        public bool IsReady => info.GetBool("isReady");
        public string LeaderId;
        
        public void SetLeader(string leaderid)
        {
            LeaderId = leaderid;
        }
    }
}