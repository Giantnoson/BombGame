using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
    public class UIMultiplayerLobbyPanel : UIBasePanel
    {
        public Button createRoomBtn;
        public Button joinRoomBtn; // Generic join or refresh
        public Transform roomListContainer;
        public GameObject roomEntryPrefab; // Simple prefab with a button
        public Button backBtn;

        private void Start()
        {
            createRoomBtn.onClick.AddListener(OnCreateRoomClick);
            joinRoomBtn.onClick.AddListener(OnRefreshListClick);
            backBtn.onClick.AddListener(OnBackClick);

            UIManager.Instance.RegisterPanel("MultiplayerLobbyPanel", this);
        }

        public override void Show()
        {
            base.Show();
            RefreshRoomList();
        }

        private void OnCreateRoomClick()
        {
            Debug.Log("Creating Room...");
            // Mock room creation
            UIManager.Instance.ShowPanel("MultiplayerRoomPanel");
            var roomPanel = UIManager.Instance.GetPanel<UIMultiplayerRoomPanel>("MultiplayerRoomPanel");
            if (roomPanel != null)
            {
                roomPanel.SetAsHost(true);
                roomPanel.AddPlayer("Me (Host)");
            }
        }

        private void OnRefreshListClick()
        {
            RefreshRoomList();
        }

        private void RefreshRoomList()
        {
            // Clear existing
            foreach (Transform child in roomListContainer)
            {
                Destroy(child.gameObject);
            }

            // Mock room list
            for (int i = 1; i <= 3; i++)
            {
                GameObject entry = Instantiate(roomEntryPrefab, roomListContainer);
                int roomId = i;
                entry.GetComponentInChildren<TMPro.TMP_Text>().text = $"Room #{roomId}";
                entry.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomId));
            }
        }

        private void JoinRoom(int roomId)
        {
            Debug.Log($"Joining Room {roomId}...");
            UIManager.Instance.ShowPanel("MultiplayerRoomPanel");
            var roomPanel = UIManager.Instance.GetPanel<UIMultiplayerRoomPanel>("MultiplayerRoomPanel");
            if (roomPanel != null)
            {
                roomPanel.SetAsHost(false);
                roomPanel.AddPlayer("Host Player");
                roomPanel.AddPlayer("Me (Guest)");
            }
        }

        private void OnBackClick()
        {
            UIManager.Instance.Back();
        }
    }
}
