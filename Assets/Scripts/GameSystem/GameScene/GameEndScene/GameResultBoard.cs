using System.Collections.Generic;
using Core.Net;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameScene.GameRuntimeScene;
using GameSystem.GameScene.MainMenu.UI;
using GameSystem.Manager;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.GameEndScene
{
    /// <summary>
    ///     游戏结算计数板，展示最终结果（击杀数、存活状态、等级等）
    ///     挂载在 WinScene / LoseScene 的 Canvas 上
    /// </summary>
    public class GameResultBoard : MonoBehaviour
    {
        [Header("标题")]
        [Tooltip("胜利/失败标题文本")]
        public TextMeshProUGUI titleText;

        [Tooltip("胜利时显示的颜色")]
        public Color victoryColor = Color.green;

        [Tooltip("失败时显示的颜色")]
        public Color defeatColor = Color.red;

        [Header("结算行容器")]
        [Tooltip("结算行文本标题")]
        public GameObject resultRowTitleText;
        [Tooltip("玩家结果行预制体")]
        public GameObject resultRowPrefab;

        [Tooltip("结果行父容器")]
        public Transform resultRowParent;

        [Header("游戏信息")]
        [Tooltip("地图名称")]
        public TextMeshProUGUI mapNameText;

        [Tooltip("游戏时长")]
        public TextMeshProUGUI durationText;

        [Tooltip("游戏模式")]
        public TextMeshProUGUI gameModeText;

        [Header("按钮")]
        
        [Tooltip("离线按钮")]
        public GameObject offlineButtonGroup;
        [Tooltip("重新开始按钮")]
        public Button restartButton;

        [Tooltip("返回角色选择按钮")]
        public Button returnToPlayerSetButton;
        
        [Tooltip("返回主菜单按钮")]
        public Button returnToMenuButton;
        
        [Tooltip("在线按钮")]
        public GameObject onlineButtonGroup;
        [Tooltip("返回房间按钮")]
        public Button returnToRoomButton;
        [Tooltip("返回地图选择按钮")]
        public Button returnToLobbyButton;


        [Header("结算面板")]
        [Tooltip("整个结算面板（可做动画）")]
        public GameObject resultPanel;

        /// <summary>
        /// 是否为随机匹配模式（在Start中从GameModeSelect读取，用于确认返回房间/退出房间的逻辑分支）
        /// </summary>
        private bool _isRandomMatch;

        private void Start()
        {
            // 检测是否为随机匹配模式（由服务器在 ENTER_BASE_GAME / GAME_OVER 中设置）
            // 此标记决定了后续"返回房间"和"返回大厅"按钮的行为分支
            _isRandomMatch = GameModeSelect.IsRandomMatch;
            
            // 优先从静态字段读取结算数据（跨场景传递，解决事件先于面板加载的问题）
            var pending = GameRuntimeSceneManager.PendingGameResult;
            if (pending != null)
            {
                ShowResult(pending);
                // 读取后清除，避免旧数据被重复使用
                GameRuntimeSceneManager.PendingGameResult = null;
            }
        }

        private void OnEnable()
        {
            if (GameModeSelect.CurrentModeType == GameModeType.Offline)
            {
                offlineButtonGroup.SetActive(true);
                onlineButtonGroup.SetActive(false);
                
                // 绑定按钮事件
                if (restartButton != null)
                    restartButton.onClick.AddListener(OnRestartClicked);

                if (returnToMenuButton != null)
                    returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
                
                if(returnToPlayerSetButton != null)
                    returnToPlayerSetButton.onClick.AddListener(OnReturnToPlayerSetClicked);
            }
            else
            {
                offlineButtonGroup.SetActive(false);
                onlineButtonGroup.SetActive(true);
                
                // 绑定按钮事件
                if (returnToRoomButton != null)
                    returnToRoomButton.onClick.AddListener(OnReturnToRoomClicked);
                if (returnToLobbyButton != null)
                    returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
            }
            
            

        }

        private void OnDisable()
        {
            
            if (GameModeSelect.CurrentModeType == GameModeType.Offline)
            {
                if (restartButton != null)
                    restartButton.onClick.RemoveListener(OnRestartClicked);

                if (returnToMenuButton != null)
                    returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
                
                if(returnToPlayerSetButton != null)
                    returnToPlayerSetButton.onClick.RemoveListener(OnReturnToPlayerSetClicked);
            }
            else
            {
                if (returnToRoomButton != null)
                    returnToRoomButton.onClick.RemoveListener(OnReturnToRoomClicked);
                if (returnToLobbyButton != null)
                    returnToLobbyButton.onClick.RemoveListener(OnReturnToLobbyClicked);
            }
        }
        
        /// <summary>
        ///     展示结算结果
        /// </summary>
        public void ShowResult(GameResultData resultData)
        {
            // 1. 显示面板
            if (resultPanel != null)
                resultPanel.SetActive(true);

            // 2. 设置标题
            if (titleText != null)
            {
                titleText.text = resultData.IsPlayerVictory ? "胜 利！" : "失 败！";
                titleText.color = resultData.IsPlayerVictory ? victoryColor : defeatColor;
            }

            // 3. 清理旧数据行
            if (resultRowParent != null)
            {
                foreach (Transform child in resultRowParent)
                    if (child != null)
                        Destroy(child.gameObject);
            }

            // 4. 生成结算行
            if (resultRowPrefab != null && resultRowParent != null)
            {
                // 排序：存活的排前面
                var sortedResults = new List<PlayerResultData>(resultData.PlayerResults);
                sortedResults.Sort((a, b) => b.IsAlive.CompareTo(a.IsAlive));
                Instantiate(resultRowTitleText, resultRowParent);
                foreach (var playerResult in sortedResults)
                {
                    var rowObj = Instantiate(resultRowPrefab, resultRowParent);
                    var rowUI = rowObj.GetComponent<ResultRowUI>();
                    if (rowUI != null)
                        rowUI.SetData(playerResult);
                }

                resultRowParent.GetComponent<ContentSizeFitter>().verticalFit =
                    ContentSizeFitter.FitMode.PreferredSize;
            }

            // 5. 显示游戏信息
            if (mapNameText != null)
                mapNameText.text = $"{resultData.MapName}";

            if (durationText != null)
            {
                var minutes = Mathf.FloorToInt(resultData.GameDuration / 60f);
                var seconds = Mathf.FloorToInt(resultData.GameDuration % 60f);
                durationText.text = $"{minutes:00}:{seconds:00}";
            }

            if (gameModeText != null)
                gameModeText.text = $"{resultData.GameMode}";
        }

        private void OnRestartClicked()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.RestartGame();
        }

        private void OnReturnToMenuClicked()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.ReturnToMainMenu();
            
        }

        private void OnReturnToPlayerSetClicked()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.ReturnToMainMenu(false);
            MainUIManager.Instance.ShowPanel(PanelSymbols.BgPanel,true);
            MainUIManager.Instance.UnHidePanel();
        }

        private void OnReturnToRoomClicked()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.ReturnToMainMenu(false);
            
            // 在 GameResultBoard 中确认：根据 _isRandomMatch 决定"返回房间"的实际行为
            if (_isRandomMatch)
            {
                // 随机匹配模式：无房间可返回，直接回到大厅
                MainUIManager.Instance.ShowPanel(PanelSymbols.BgPanel, true);
                MainUIManager.Instance.Back();

            }
            else
            {
                // 房间模式：服务器已在游戏结束时自动将玩家返回房间状态，
                // 客户端只需恢复房间面板并请求刷新最新的房间信息
                MainUIManager.Instance.ShowPanel(PanelSymbols.BgPanel, true);
                MainUIManager.Instance.GetPanel<MainUIMultiplayerRoomPanel>(PanelSymbols.MultiPlayerRoomPanel).Show();
                TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameReqRoomInfo));
            }
        }

        private void OnReturnToLobbyClicked()
        {
            // 在 GameResultBoard 中确认：房间模式下需主动退出房间，随机匹配无需退房
            if (!_isRandomMatch)
            {
                TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameLeaveRoom));
            }
            
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.ReturnToMainMenu(false);
            MainUIManager.Instance.ShowPanel(PanelSymbols.BgPanel, true);
            MainUIManager.Instance.Back();
        }
        
        
    }
}
