using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class UIMultiplayerLoginPanel : UIBasePanel
    {
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginBtn;
        public Button backBtn;

        private void Start()
        {
            loginBtn.onClick.AddListener(OnLoginClick);
            backBtn.onClick.AddListener(OnBackClick);

            UIManager.Instance.RegisterPanel("MultiplayerLoginPanel", this);
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
            UIManager.Instance.ShowPanel("MultiplayerLobbyPanel");
        }

        private void OnBackClick()
        {
            UIManager.Instance.Back();
        }
    }
}
