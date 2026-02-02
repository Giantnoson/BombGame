using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GameSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIMultiplayerRoomPanel : UIBasePanel
    {
        public Transform playerListContainer;
        public GameObject playerEntryPrefab;
        public Button startBtn;
        public Button leaveBtn;
        
        private bool _isHost = false;
        private List<string> _players = new List<string>();

        private void Start()
        {
            startBtn.onClick.AddListener(OnStartClick);
            leaveBtn.onClick.AddListener(OnLeaveClick);

            UIManager.Instance.RegisterPanel("MultiplayerRoomPanel", this);
        }

        public void SetAsHost(bool isHost)
        {
            _isHost = isHost;
            startBtn.gameObject.SetActive(isHost);
            _players.Clear();
            UpdateUI();
        }

        public void AddPlayer(string name)
        {
            if (!_players.Contains(name))
            {
                _players.Add(name);
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            // Clear existing
            foreach (Transform child in playerListContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate
            foreach (var player in _players)
            {
                GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
                entry.GetComponentInChildren<TMP_Text>().text = player;
            }
        }

        private void OnStartClick()
        {
            if (!_isHost) return;

            Debug.Log("Starting Online Game...");
            if (GameModeSelect.Instance != null)
            {
                GameModeSelect.Instance.SetGameMode(GameModeType.Online, EnhancedGameFlowManager.GameState.Prepare, _players.Count, 0);
            }
            SceneManager.LoadScene("GameScene");
        }

        private void OnLeaveClick()
        {
            UIManager.Instance.Back();
        }
    }
}
