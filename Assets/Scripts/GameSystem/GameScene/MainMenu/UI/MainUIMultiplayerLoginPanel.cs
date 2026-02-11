using System.Net.Sockets;
using Core.Net;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMultiplayerLoginPanel : UIBasePanel
    {
        public string panelName = "MultiplayerLoginPanel";
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginBtn;
        public Button backBtn;

        private void Start()
        {
            loginBtn.onClick.AddListener(OnLoginClick);
            backBtn.onClick.AddListener(OnBackClick);

            MainUIManager.Instance.RegisterPanel(panelName, this);
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

            // Mock login success
            Debug.Log($"Logging in as {username}...");
            TcpGameClient.Instance.TcpStart(username, password);
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
