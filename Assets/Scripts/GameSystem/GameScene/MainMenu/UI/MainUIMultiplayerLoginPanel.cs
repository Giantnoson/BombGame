using Config;
using Core.Net;
using GameSystem.Message;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu.UI
{
    public class MainUIMultiplayerLoginPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerLoginPanel;
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginBtn;
        public Button backBtn;

        public override void Show()
        {
            base.Show();
            usernameInput.text = OnlineConfig.Instance.defaultPlayerName;
            passwordInput.text = OnlineConfig.Instance.defaultPlayerPassword;
            loginBtn.onClick.AddListener(OnLoginClick);
            backBtn.onClick.AddListener(OnBackClick);
        }

        public override void Hide()
        {
            base.Hide();
            loginBtn.onClick.RemoveListener(OnLoginClick);
            backBtn.onClick.RemoveListener(OnBackClick);
        }

        private void OnLoginClick()
        {
            string username = usernameInput.text;
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning("Username cannot be empty");
                GlobalMessageManager.Instance.SendTopMessage(MessageType.System, MessageLevel.Warning, "用户名不能为空");
                return;
            }

            Debug.Log($"Logging in as {username}...");
            TcpGameClient.Instance.TcpStart(username, password);
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
