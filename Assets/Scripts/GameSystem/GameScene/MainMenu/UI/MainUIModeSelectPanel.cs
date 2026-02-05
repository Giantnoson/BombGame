using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIModeSelectPanel : UIBasePanel
    {
        public string panelName = "ModeSelectPanel";
        public Button singlePlayerButton;
        public Button multiplayerButton;
        public Button backButton;

        private void Start()
        {
            singlePlayerButton.onClick.AddListener(OnSinglePlayerClick);
            multiplayerButton.onClick.AddListener(OnMultiplayerClick);
            backButton.onClick.AddListener(OnBackClick);

            MainUIManager.Instance.RegisterPanel(panelName, this);
        }
        
        private void OnSinglePlayerClick()
        {
            MainUIManager.Instance.ShowPanel("MapSelectPanel");
        }

        private void OnMultiplayerClick()
        {
            MainUIManager.Instance.ShowPanel("MultiplayerLoginPanel");
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
