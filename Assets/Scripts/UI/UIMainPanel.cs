using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMainPanel : UIBasePanel
    {
        public Button startButton;
        public Button quitButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
            quitButton.onClick.AddListener(OnQuitClick);
            
            UIManager.Instance.RegisterPanel("MainPanel", this);
        }

        private void OnStartClick()
        {
            UIManager.Instance.ShowPanel("ModeSelectPanel");
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
