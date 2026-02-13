using Config;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameSystem.GameScene.MainMenu;
using GameSystem.UI;
using UnityEngine.SceneManagement;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUISinglePlayerPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.SinglePlayerPanel;
        
        public TMP_Text playerCountText;
        public TMP_Text npcCountText;
        
        public Button addPlayerBtn;
        public Button subPlayerBtn;
        public Button addNpcBtn;
        public Button subNpcBtn;
        
        public Button startBtn;
        public Button backBtn;

        private int _playerCount = 1;
        private int _npcCount = 0;

        private void Start()
        {
            addPlayerBtn.onClick.AddListener(() => ChangePlayerCount(1));
            subPlayerBtn.onClick.AddListener(() => ChangePlayerCount(-1));
            addNpcBtn.onClick.AddListener(() => ChangeNpcCount(1));
            subNpcBtn.onClick.AddListener(() => ChangeNpcCount(-1));
            
            startBtn.onClick.AddListener(OnStartClick);
            backBtn.onClick.AddListener(OnBackClick);

            UpdateUI();
        }

        private void ChangePlayerCount(int delta)
        {
            int nextCount = Mathf.Clamp(_playerCount + delta, 1, 4);
            if (nextCount + _npcCount <= 4)
            {
                _playerCount = nextCount;
            }
            UpdateUI();
        }

        private void ChangeNpcCount(int delta)
        {
            int nextCount = Mathf.Clamp(_npcCount + delta, 0, 3);
            if (nextCount + _playerCount <= 4)
            {
                _npcCount = nextCount;
            }
            UpdateUI();
        }

        private void UpdateUI()
        {
            playerCountText.text = $"玩家数量: {_playerCount}";
            npcCountText.text = $"NPC数量: {_npcCount}";
            
            // Validate start button (though logic above already ensures it)
            startBtn.interactable = (_playerCount + _npcCount <= 4);
        }

        private void OnStartClick()
        {
            // Save settings to GameModeSelect
            if (GameModeSelect.Instance != null)
            {
                GameModeSelect.Instance.SetGameMode(GameModeType.Offline, _playerCount, _npcCount);
                MainUIManager.Instance.ShowPanel(PanelSymbols.PlaySettingPanel);
            }
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
