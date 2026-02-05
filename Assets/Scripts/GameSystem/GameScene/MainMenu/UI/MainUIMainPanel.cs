using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMainPanel : UIBasePanel
    {
        public string panelName = "MainPanel";
        public Button startButton;
        public Button quitButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
            quitButton.onClick.AddListener(OnQuitClick);
            
            MainUIManager.Instance.RegisterPanel(panelName, this);
        }

        private void OnStartClick()
        {
            MainUIManager.Instance.ShowPanel("ModeSelectPanel");
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
