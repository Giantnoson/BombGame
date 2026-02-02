using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// This script is an optional helper to quickly build the UI hierarchy in the Editor.
    /// Attach it to a Canvas and run the Context Menu command.
    /// </summary>
    public class UIGenerator : MonoBehaviour
    {
        [ContextMenu("Generate Basic UI Hierarchy")]
        public void Generate()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("UIGenerator must be attached to a Canvas!");
                return;
            }

            // Create UIController
            GameObject controllerGo = new GameObject("UIController");
            controllerGo.transform.SetParent(transform, false);
            UIController controller = controllerGo.AddComponent<UIController>();

            // Create Panels Container
            GameObject panelsGo = new GameObject("Panels");
            panelsGo.transform.SetParent(controllerGo.transform, false);
            RectTransform panelsRect = panelsGo.AddComponent<RectTransform>();
            panelsRect.anchorMin = Vector2.zero;
            panelsRect.anchorMax = Vector2.one;
            panelsRect.sizeDelta = Vector2.zero;

            // 1. Main Panel
            controller.mainPanel = CreatePanel<UIMainPanel>(panelsRect, "MainPanel");
            controller.mainPanel.startButton = CreateButton(controller.mainPanel.transform, "StartButton", "开始游戏", new Vector2(0, 50));
            controller.mainPanel.quitButton = CreateButton(controller.mainPanel.transform, "QuitButton", "退出", new Vector2(0, -50));

            // 2. Mode Select Panel
            controller.modeSelectPanel = CreatePanel<UIModeSelectPanel>(panelsRect, "ModeSelectPanel");
            controller.modeSelectPanel.singlePlayerButton = CreateButton(controller.modeSelectPanel.transform, "SingleBtn", "单人游戏", new Vector2(0, 50));
            controller.modeSelectPanel.multiplayerButton = CreateButton(controller.modeSelectPanel.transform, "MultiBtn", "多人游戏", new Vector2(0, -50));
            controller.modeSelectPanel.backButton = CreateButton(controller.modeSelectPanel.transform, "BackBtn", "返回", new Vector2(-300, 200), new Vector2(100, 50));

            // 3. Single Player Panel
            controller.singlePlayerPanel = CreatePanel<UISinglePlayerPanel>(panelsRect, "SinglePlayerPanel");
            controller.singlePlayerPanel.playerCountText = CreateText(controller.singlePlayerPanel.transform, "PlayerText", "玩家数量: 1", new Vector2(0, 100));
            controller.singlePlayerPanel.npcCountText = CreateText(controller.singlePlayerPanel.transform, "NpcText", "NPC数量: 0", new Vector2(0, 50));
            controller.singlePlayerPanel.addPlayerBtn = CreateButton(controller.singlePlayerPanel.transform, "AddP", "+", new Vector2(150, 100), new Vector2(40, 40));
            controller.singlePlayerPanel.subPlayerBtn = CreateButton(controller.singlePlayerPanel.transform, "SubP", "-", new Vector2(-150, 100), new Vector2(40, 40));
            controller.singlePlayerPanel.addNpcBtn = CreateButton(controller.singlePlayerPanel.transform, "AddN", "+", new Vector2(150, 50), new Vector2(40, 40));
            controller.singlePlayerPanel.subNpcBtn = CreateButton(controller.singlePlayerPanel.transform, "SubN", "-", new Vector2(-150, 50), new Vector2(40, 40));
            controller.singlePlayerPanel.startBtn = CreateButton(controller.singlePlayerPanel.transform, "Start", "进入游戏", new Vector2(0, -100));
            controller.singlePlayerPanel.backBtn = CreateButton(controller.singlePlayerPanel.transform, "Back", "返回", new Vector2(-300, 200), new Vector2(100, 50));

            // 4. Login Panel
            controller.loginPanel = CreatePanel<UIMultiplayerLoginPanel>(panelsRect, "LoginPanel");
            CreateText(controller.loginPanel.transform, "Title", "多人登录", new Vector2(0, 150));
            controller.loginPanel.usernameInput = CreateInputField(controller.loginPanel.transform, "User", "用户名...", new Vector2(0, 50));
            controller.loginPanel.passwordInput = CreateInputField(controller.loginPanel.transform, "Pass", "密码...", new Vector2(0, -20));
            controller.loginPanel.loginBtn = CreateButton(controller.loginPanel.transform, "Login", "登录", new Vector2(0, -100));
            controller.loginPanel.backBtn = CreateButton(controller.loginPanel.transform, "Back", "返回", new Vector2(-300, 200), new Vector2(100, 50));

            // 5. Lobby Panel
            controller.lobbyPanel = CreatePanel<UIMultiplayerLobbyPanel>(panelsRect, "LobbyPanel");
            controller.lobbyPanel.createRoomBtn = CreateButton(controller.lobbyPanel.transform, "Create", "创建房间", new Vector2(-200, 150));
            controller.lobbyPanel.joinRoomBtn = CreateButton(controller.lobbyPanel.transform, "Refresh", "刷新列表", new Vector2(200, 150));
            controller.lobbyPanel.backBtn = CreateButton(controller.lobbyPanel.transform, "Back", "返回", new Vector2(-300, 200), new Vector2(100, 50));
            
            GameObject scrollGo = new GameObject("RoomList");
            scrollGo.transform.SetParent(controller.lobbyPanel.transform, false);
            RectTransform scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(600, 300);
            controller.lobbyPanel.roomListContainer = scrollGo.transform;

            // 6. Room Panel
            controller.roomPanel = CreatePanel<UIMultiplayerRoomPanel>(panelsRect, "RoomPanel");
            controller.roomPanel.startBtn = CreateButton(controller.roomPanel.transform, "Start", "开始游戏", new Vector2(0, -150));
            controller.roomPanel.leaveBtn = CreateButton(controller.roomPanel.transform, "Leave", "离开房间", new Vector2(-300, 200), new Vector2(100, 50));
            controller.roomPanel.playerListContainer = new GameObject("PlayerList").transform;
            controller.roomPanel.playerListContainer.SetParent(controller.roomPanel.transform, false);

            Debug.Log("UI Hierarchy Generated! Please assign prefabs for roomEntry and playerEntry manually.");
        }

        private T CreatePanel<T>(Transform parent, string name) where T : UIBasePanel
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            Image img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);
            
            return go.AddComponent<T>();
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size = default)
        {
            if (size == default) size = new Vector2(160, 45);
            
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            
            go.AddComponent<Image>();
            Button btn = go.AddComponent<Button>();
            
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            TMP_Text t = textGo.AddComponent<TextMeshProUGUI>();
            t.text = label;
            t.color = Color.black;
            t.alignment = TextAlignmentOptions.Center;
            ((RectTransform)textGo.transform).sizeDelta = size;
            
            return btn;
        }

        private TMP_Text CreateText(Transform parent, string name, string content, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(300, 50);
            
            TMP_Text t = go.AddComponent<TextMeshProUGUI>();
            t.text = content;
            t.alignment = TextAlignmentOptions.Center;
            return t;
        }

        private TMP_InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(200, 40);
            go.AddComponent<Image>();
            
            TMP_InputField input = go.AddComponent<TMP_InputField>();
            
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            TMP_Text t = textGo.AddComponent<TextMeshProUGUI>();
            t.color = Color.black;
            ((RectTransform)textGo.transform).sizeDelta = new Vector2(190, 30);
            
            input.textComponent = t;
            return input;
        }
    }
}
