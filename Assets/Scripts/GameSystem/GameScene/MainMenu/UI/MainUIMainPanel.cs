using GameSystem.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMainPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MainPanel;
        public Button startButton;
        public Button quitButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
            quitButton.onClick.AddListener(OnQuitClick);
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
