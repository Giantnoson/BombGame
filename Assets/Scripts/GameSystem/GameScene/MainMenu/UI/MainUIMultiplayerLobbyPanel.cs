using GameSystem.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMultiplayerLobbyPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerLobbyPanel;
        public Button createRoomBtn;
        public Button joinRoomBtn; // Generic join or refresh
        public Transform roomListContainer;
        /*
        public GameObject roomEntryPrefab; // Simple prefab with a button
        */
        public Button backBtn;
        public Button RandomFitBtn;

        private void Start()
        {
            createRoomBtn.onClick.AddListener(OnCreateRoomClick);
            joinRoomBtn.onClick.AddListener(OnRefreshListClick);
            backBtn.onClick.AddListener(OnBackClick);
            RandomFitBtn.onClick.AddListener(OnRandomFitClick);
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
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerPlaySetPanel);
            var roomPanel = MainUIManager.Instance.GetPanel<MainUIMultiplayerRoomPanel>(PanelSymbols.MultiPlayerRoomPanel);
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
            /*for (int i = 1; i <= 3; i++)
            {
                GameObject entry = Instantiate(roomEntryPrefab, roomListContainer);
                int roomId = i;
                entry.GetComponentInChildren<TMPro.TMP_Text>().text = $"Room #{roomId}";
                entry.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomId));
            }*/
        }

        private void OnRandomFitClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerPlaySetPanel);
        }
        private void JoinRoom(int roomId)
        {
            Debug.Log($"Joining Room {roomId}...");
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerRoomPanel);
            var roomPanel = MainUIManager.Instance.GetPanel<MainUIMultiplayerRoomPanel>(PanelSymbols.MultiPlayerRoomPanel);
            if (roomPanel != null)
            {
                roomPanel.SetAsHost(false);
                roomPanel.AddPlayer("Host Player");
                roomPanel.AddPlayer("Me (Guest)");
            }
        }

        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}
