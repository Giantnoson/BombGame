using GameSystem.UI;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu.UI
{
    public class MainUIModeSelectPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.ModeSelectPanel;
        public Button singlePlayerButton;
        public Button multiplayerButton;
        public Button backButton;

        private void Start()
        {
            singlePlayerButton.onClick.AddListener(OnSinglePlayerClick);
            multiplayerButton.onClick.AddListener(OnMultiplayerClick);
            backButton.onClick.AddListener(OnBackClick);

        }
        
        private void OnSinglePlayerClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MapSelectPanel);
        }

        private void OnMultiplayerClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerLoginPanel);
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
