using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIController : MonoBehaviour
    {
        [Header("UI面板")]
        public MainUIMainPanel mainPanel;
        public MainUIModeSelectPanel modeSelectPanel;
        public MainUISinglePlayerPanel singlePlayerPanel;
        public MainUIMultiplayerLoginPanel loginPanel;
        public MainUIMultiplayerLobbyPanel lobbyPanel;
        public MainUIMultiplayerRoomPanel roomPanel;
        public MainUIBGPanel bg;
        public MainUIMapSelectPanel mapSelectPanel;
        public MainUIPlaySetPanel playSetPanel;

        private void Start()
        {
            // 初始化UI管理器
            var manager = MainUIManager.Instance;
            // 注册面板
            manager.RegisterPanel(mainPanel.panelName, mainPanel);
            manager.RegisterPanel(modeSelectPanel.panelName, modeSelectPanel);
            manager.RegisterPanel(singlePlayerPanel.panelName, singlePlayerPanel);
            manager.RegisterPanel(loginPanel.panelName, loginPanel);
            manager.RegisterPanel(lobbyPanel.panelName, lobbyPanel);
            manager.RegisterPanel(roomPanel.panelName, roomPanel);
            manager.RegisterPanel(bg.panelName, bg);
            manager.RegisterPanel(mapSelectPanel.panelName, mapSelectPanel);
            manager.RegisterPanel(playSetPanel.panelName, playSetPanel);
            // 隐藏所有面板
            manager.CloseAll();

            //展示需要展示的面板
            manager.ShowPanel("MainPanel");
            manager.ShowDontHidePanel("BG");
        }

        // 设置引用的辅助方法,前提是在同一对象上
        [ContextMenu("自动加载UI参照")]
        public void AutoSetup()
        {
            mainPanel = GetComponentInChildren<MainUIMainPanel>(true);
            modeSelectPanel = GetComponentInChildren<MainUIModeSelectPanel>(true);
            singlePlayerPanel = GetComponentInChildren<MainUISinglePlayerPanel>(true);
            loginPanel = GetComponentInChildren<MainUIMultiplayerLoginPanel>(true);
            lobbyPanel = GetComponentInChildren<MainUIMultiplayerLobbyPanel>(true);
            roomPanel = GetComponentInChildren<MainUIMultiplayerRoomPanel>(true);
            bg = GetComponentInChildren<MainUIBGPanel>(true);
            mapSelectPanel = GetComponentInChildren<MainUIMapSelectPanel>(true);
            playSetPanel = GetComponentInChildren<MainUIPlaySetPanel>(true);
        }
    }
}
