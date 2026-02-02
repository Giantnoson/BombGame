using GameSystem.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene
{
    /// <summary>
    ///     示例游戏场景管理器
    ///     展示如何实现BaseSceneManager
    /// </summary>
    public class ExampleGameScene : BaseSceneManager
    {
        [Header("游戏场景UI")] [SerializeField] private Button pauseButton;

        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;

        [Header("游戏设置")] [SerializeField] private int enemiesToDefeat = 5;

        [SerializeField] private int currentEnemiesDefeated;

        protected override void Awake()
        {
            base.Awake();

            // 如果场景名称为空，设置为默认值
            if (string.IsNullOrEmpty(sceneName)) sceneName = "GameScene";
        }

        private void Update()
        {
            // 更新计时器UI
            if (timerText != null && EnhancedGameFlowManager.Instance.IsGameActive)
            {
                var time = EnhancedGameFlowManager.Instance.GameTimer;
                var minutes = Mathf.FloorToInt(time / 60);
                var seconds = Mathf.FloorToInt(time % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        public override void InitializeScene()
        {
            Debug.Log($"初始化游戏场景: {sceneName}");

            // 重置游戏状态
            currentEnemiesDefeated = 0;
            isSceneCompleted = false;
            isSceneSuccessful = false;

            // 设置UI
            if (levelText != null) levelText.text = $"关卡 {EnhancedGameFlowManager.Instance.CurrentLevelIndex + 1}";

            // 添加按钮事件监听
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseButtonClicked);

            if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClicked);

            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

            // 订阅游戏事件
            GameEventSystem.AddListener<GameEvents.TimeUpEvent>(OnTimeUp);
        }

        public override void CleanupScene()
        {
            Debug.Log($"清理游戏场景: {sceneName}");

            // 移除按钮事件监听
            if (pauseButton != null) pauseButton.onClick.RemoveListener(OnPauseButtonClicked);

            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartButtonClicked);

            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);

            // 取消订阅游戏事件
            GameEventSystem.RemoveListener<GameEvents.TimeUpEvent>(OnTimeUp);
        }

        public override void PauseScene()
        {
            Debug.Log($"游戏场景暂停: {sceneName}");
            // 可以在这里添加暂停逻辑，比如暂停动画、音效等
        }

        public override void ResumeScene()
        {
            Debug.Log($"游戏场景恢复: {sceneName}");
            // 可以在这里添加恢复逻辑，比如恢复动画、音效等
        }

        /// <summary>
        ///     敌人被击败时调用
        /// </summary>
        public void OnEnemyDefeated()
        {
            currentEnemiesDefeated++;
            Debug.Log($"击败敌人: {currentEnemiesDefeated}/{enemiesToDefeat}");

            // 检查是否完成关卡
            if (currentEnemiesDefeated >= enemiesToDefeat && !isSceneCompleted) CompleteScene(true);
        }

        #region Game Event Handlers

        private void OnTimeUp(GameEvents.TimeUpEvent evt)
        {
            // 时间用尽，游戏失败
            if (!isSceneCompleted) CompleteScene(false);
        }

        #endregion

        #region Button Event Handlers

        private void OnPauseButtonClicked()
        {
            GameEventSystem.Broadcast(new GameEvents.PauseGameEvent());
        }

        private void OnRestartButtonClicked()
        {
            GameEventSystem.Broadcast(new GameEvents.RestartGameEvent());
        }

        private void OnMainMenuButtonClicked()
        {
            GameEventSystem.Broadcast(new GameEvents.ReturnToMainMenuEvent());
        }

        #endregion
    }
}