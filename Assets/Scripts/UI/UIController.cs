using UnityEngine;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [Header("UI面板")]
        public UIMainPanel mainPanel;
        public UIModeSelectPanel modeSelectPanel;
        public UISinglePlayerPanel singlePlayerPanel;
        public UIMultiplayerLoginPanel loginPanel;
        public UIMultiplayerLobbyPanel lobbyPanel;
        public UIMultiplayerRoomPanel roomPanel;

        private void Start()
        {
            // Ensure UIManager exists
            var manager = UIManager.Instance;

            // Hide all panels initially
            manager.CloseAll();

            // Show main panel
            manager.ShowPanel("MainPanel");
        }

        // Helper method to setup references if they are on the same object or children
        [ContextMenu("Auto Setup References")]
        public void AutoSetup()
        {
            mainPanel = GetComponentInChildren<UIMainPanel>(true);
            modeSelectPanel = GetComponentInChildren<UIModeSelectPanel>(true);
            singlePlayerPanel = GetComponentInChildren<UISinglePlayerPanel>(true);
            loginPanel = GetComponentInChildren<UIMultiplayerLoginPanel>(true);
            lobbyPanel = GetComponentInChildren<UIMultiplayerLobbyPanel>(true);
            roomPanel = GetComponentInChildren<UIMultiplayerRoomPanel>(true);
        }
    }
}
