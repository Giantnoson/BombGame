using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIModeSelectPanel : UIBasePanel
    {
        public Button singlePlayerButton;
        public Button multiplayerButton;
        public Button backButton;

        private void Start()
        {
            singlePlayerButton.onClick.AddListener(OnSinglePlayerClick);
            multiplayerButton.onClick.AddListener(OnMultiplayerClick);
            backButton.onClick.AddListener(OnBackClick);

            UIManager.Instance.RegisterPanel("ModeSelectPanel", this);
        }

        private void OnSinglePlayerClick()
        {
            UIManager.Instance.ShowPanel("SinglePlayerPanel");
        }

        private void OnMultiplayerClick()
        {
            UIManager.Instance.ShowPanel("MultiplayerLoginPanel");
        }

        private void OnBackClick()
        {
            UIManager.Instance.Back();
        }
    }
}
