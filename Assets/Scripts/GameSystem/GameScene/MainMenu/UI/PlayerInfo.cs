using Config;
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
        
        public bool isRoomOwner = false;


        public void SetPlayerInfo(string playerNameText, string playerTypeText, string playerStatusText)
        {
            this.playerNameText.text = playerNameText;
            this.playerTypeText.text = playerTypeText;
            this.playerStatusText.text = playerStatusText;
        }

        public void SetPlayerInfo(string playerNameText)
        {
            this.playerNameText.text = playerNameText;
        }

        public void SetPlayerInfo(string playerNameText, string playerTypeText)
        {
            this.playerNameText.text = playerNameText;
            this.playerTypeText.text = playerTypeText;
        }

        public void SetPlayerInfo(string playerNameText, string playerTypeText, string playerStatusText,
            bool isRoomOwner)
        {
            this.playerNameText.text = playerNameText;
            this.playerTypeText.text = playerTypeText;
            this.playerStatusText.text = playerStatusText;
            if (isRoomOwner)
            {
                //transferRoomOwner.gameObject.SetActive(true);
                removePlayer.gameObject.SetActive(true);
            }
        }
        
        
        
        
        public PlayerInfo(string playerNameText, bool isRoomOwner) : this(playerNameText,CharacterType.Balance,"准备中")
        {
            
            this.playerNameText.text = playerNameText;
        }

        public PlayerInfo(string playerNameText, CharacterType playerTypeText, string playerStatusText)
        {
            this.playerNameText.text = playerNameText;
            this.playerTypeText.text = playerTypeText.ToString();
            this.playerStatusText.text = playerStatusText;
        }

        private void Init()
        {
            if (playerNameText == null)
            {
                Debug.LogError("playerNameText为空");
            }

            if (playerTypeText == null)
            {
                Debug.LogError("playerTypeText为空");
            }

            if (playerStatusText == null)
            {
                Debug.LogError("playerStatusText为空");
            }
            if (playerBtnTran == null)
            {
                Debug.LogError("playerBtnTran为空");
            }

            if (removePlayer == null)
            {
                Debug.LogError("removePlayer为空");
            }
            
            if (transferRoomOwner == null)
            {
                Debug.LogError("transferRoomOwner为空");
            }
            
        }

        private void OnRemovePlayer()
        {
            Debug.Log("移除玩家");
        }

        private void OnTransferRoomOwner()
        {
            Debug.Log("转让房主");
        }
        
        
        
        
        
        

    }
}