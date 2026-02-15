using System.Net.Sockets;
using Core.Net;
using GameSystem.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMultiplayerLoginPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerLoginPanel;
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginBtn;
        public Button backBtn;

        private void Start()
        {
            usernameInput.text = "gql";
            passwordInput.text = "123456";
            loginBtn.onClick.AddListener(OnLoginClick);
            backBtn.onClick.AddListener(OnBackClick);
        }

        private void OnLoginClick()
        {
            string username = usernameInput.text;
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning("Username cannot be empty");
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
