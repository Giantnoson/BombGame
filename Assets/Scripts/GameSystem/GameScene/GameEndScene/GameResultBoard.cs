using System.Collections.Generic;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameScene.GameRuntimeScene;
using GameSystem.Manager;
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
        [Tooltip("重新开始按钮")]
        public Button restartButton;

        [Tooltip("返回主菜单按钮")]
        public Button returnToMenuButton;

        [Header("结算面板")]
        [Tooltip("整个结算面板（可做动画）")]
        public GameObject resultPanel;

        private void Start()
        {
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
            // 绑定按钮事件
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }

        private void OnDisable()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (returnToMenuButton != null)
                returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
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
    }
}
