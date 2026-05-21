using UnityEngine;
using GameSystem.UI;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu.UI
{
    public class MainUIMainPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MainPanel;
        public Button startButton;
        public Button quitButton;

        public override void Show()
        {
            base.Show();
            startButton.onClick.AddListener(OnStartClick);
            quitButton.onClick.AddListener(OnQuitClick);
        }

        public override void Hide()
        {
            base.Hide();
            startButton.onClick.RemoveListener(OnStartClick);
            quitButton.onClick.RemoveListener(OnQuitClick);
        }

        private void OnStartClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.ModeSelectPanel);
        }

        private void OnQuitClick()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
