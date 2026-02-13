using System.Collections.Generic;
using GameSystem.GameScene.MainMenu.UI;
using GameSystem.UI;
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
        public MainUIMultiPlayerMapSelectPanel multiPlayerMapSelectPanel;

        private void Start()
        {
            // 初始化UI管理器
            var manager = MainUIManager.Instance;
            // 注册面板
            manager.RegisterPanel(mainPanel);
            manager.RegisterPanel(modeSelectPanel);
            manager.RegisterPanel(singlePlayerPanel);
            manager.RegisterPanel(loginPanel);
            manager.RegisterPanel(lobbyPanel);
            manager.RegisterPanel(roomPanel);
            manager.RegisterPanel(bg);
            manager.RegisterPanel(mapSelectPanel);
            manager.RegisterPanel(playSetPanel);
            manager.RegisterPanel(multiPlayerMapSelectPanel);
            // 隐藏所有面板
            manager.CloseAll();

            //展示需要展示的面板
            manager.ShowPanel(PanelSymbols.MainPanel);
            manager.ShowDontHidePanel(PanelSymbols.BgPanel);
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
            multiPlayerMapSelectPanel = GetComponentInChildren<MainUIMultiPlayerMapSelectPanel>(true);
        }
    }
}
