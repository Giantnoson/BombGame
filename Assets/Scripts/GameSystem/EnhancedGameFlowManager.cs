using System;
using System.Collections.Generic;
using GameSystem.EventSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    /// <summary>
    ///     增强版游戏流管理器，负责管理全局游戏状态和场景转换
    /// </summary>
    public class EnhancedGameFlowManager : MonoBehaviour
    {
        #region Cleanup

        private void OnDestroy()
        {
            // 取消事件监听
            UnregisterEventListeners();

            // 取消场景加载事件
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        #endregion

        #region 单例模式

        public static EnhancedGameFlowManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 游戏状态

        /// <summary>
        ///     游戏状态枚举
        /// </summary>
        public enum GameState
        {
            MainMenu, // 主菜单
            Loading, // 加载中
            Playing, // 游戏进行中
            Paused, // 游戏暂停
            GameOver, // 游戏结束
            Victory, // 游戏胜利
            Settings, // 设置界面
            Prepare
        }

        // 当前游戏状态
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // 之前的状态，用于状态恢复
        private GameState previousState = GameState.MainMenu;

        #endregion

        #region 场景管理

        // 场景信息类
        [Serializable]
        public class SceneInfo
        {
            public string sceneName;
            public GameState associatedState;
            public bool isAdditive; // 是否以叠加方式加载场景
        }

        [Header("场景配置")] [SerializeField] private List<SceneInfo> sceneInfos = new();

        // 当前加载的场景
        private readonly List<string> loadedScenes = new();

        // 场景加载状态
        public bool IsSceneLoading { get; private set; }

        #endregion

        #region 游戏配置

        [Header("游戏配置")] [SerializeField] private float gameDuration = 180f; // 游戏时长（秒）

        [SerializeField] private int currentLevelIndex; // 当前关卡索引

        public float GameTimer { get; private set; }

        public bool IsGameActive { get; private set; }

        public int CurrentLevelIndex => currentLevelIndex;

        #endregion

        #region 事件

        // 游戏状态变化事件
        public static event Action<GameState, GameState> OnGameStateChanged; // (旧状态, 新状态)

        // 场景加载事件
        public static event Action<string> OnSceneLoadStarted; // 场景名
        public static event Action<string> OnSceneLoadCompleted; // 场景名

        #endregion

        #region 初始化

        private void Initialize()
        {
            // 订阅场景加载事件
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            // 设置初始状态
            ChangeGameState(GameState.MainMenu);

            // 注册事件监听
            RegisterEventListeners();
        }

        private void RegisterEventListeners()
        {
            // 监听游戏相关事件
            GameEventSystem.AddListener<GameEvents.PauseGameEvent>(OnPauseGameRequested);
            GameEventSystem.AddListener<GameEvents.ResumeGameEvent>(OnResumeGameRequested);
            GameEventSystem.AddListener<GameEvents.RestartGameEvent>(OnRestartGameRequested);
            GameEventSystem.AddListener<GameEvents.ReturnToMainMenuEvent>(OnReturnToMainMenuRequested);
            GameEventSystem.AddListener<GameEvents.QuitGameEvent>(OnQuitGameRequested);
            GameEventSystem.AddListener<GameEvents.LoadLevelEvent>(OnLoadLevelRequested);
            GameEventSystem.AddListener<GameEvents.LevelCompletedEvent>(OnLevelCompleted);
        }

        private void UnregisterEventListeners()
        {
            // 取消事件监听
            GameEventSystem.RemoveListener<GameEvents.PauseGameEvent>(OnPauseGameRequested);
            GameEventSystem.RemoveListener<GameEvents.ResumeGameEvent>(OnResumeGameRequested);
            GameEventSystem.RemoveListener<GameEvents.RestartGameEvent>(OnRestartGameRequested);
            GameEventSystem.RemoveListener<GameEvents.ReturnToMainMenuEvent>(OnReturnToMainMenuRequested);
            GameEventSystem.RemoveListener<GameEvents.QuitGameEvent>(OnQuitGameRequested);
            GameEventSystem.RemoveListener<GameEvents.LoadLevelEvent>(OnLoadLevelRequested);
            GameEventSystem.RemoveListener<GameEvents.LevelCompletedEvent>(OnLevelCompleted);
        }

        #endregion

        #region Update

        private void Update()
        {
            UpdateGameTimer();
            HandleInput();
        }

        private void UpdateGameTimer()
        {
            if (CurrentState == GameState.Playing && IsGameActive)
            {
                GameTimer -= Time.deltaTime;

                // 检查时间是否用完
                if (GameTimer <= 0)
                {
                    GameEventSystem.Broadcast(new GameEvents.TimeUpEvent());
                    EndGame(false);
                }
            }
        }

        private void HandleInput()
        {
            // 处理ESC键
            if (Input.GetKeyDown(KeyCode.Escape))
                switch (CurrentState)
                {
                    case GameState.Playing:
                        PauseGame();
                        break;
                    case GameState.Paused:
                        ResumeGame();
                        break;
                    case GameState.Settings:
                        // 从设置界面返回到之前的界面
                        ChangeGameState(previousState);
                        break;
                }
        }

        #endregion

        #region 游戏状态管理

        /// <summary>
        ///     改变游戏状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void ChangeGameState(GameState newState)
        {
            if (CurrentState == newState) return;

            var oldState = CurrentState;
            previousState = CurrentState;
            CurrentState = newState;

            // 根据状态执行相应操作
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    IsGameActive = false;
                    LoadSceneForState(GameState.MainMenu);
                    break;

                case GameState.Loading:
                    Time.timeScale = 1f;
                    IsGameActive = false;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    IsGameActive = true;
                    if (oldState != GameState.Paused)
                    {
                        GameTimer = gameDuration;
                        // 如果从非游戏状态进入，加载当前关卡
                        if (oldState != GameState.Playing) LoadSceneForState(GameState.Playing);
                    }

                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    IsGameActive = false;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    IsGameActive = false;
                    break;

                case GameState.Victory:
                    Time.timeScale = 0f;
                    IsGameActive = false;
                    break;

                case GameState.Settings:
                    // 保存当前时间尺度，以便从设置返回时恢复
                    break;
            }

            // 广播状态变化事件
            OnGameStateChanged?.Invoke(oldState, newState);
            GameEventSystem.Broadcast(new GameEvents.GameStateChangedEvent(oldState, newState));
        }

        /// <summary>
        ///     开始游戏
        /// </summary>
        public void StartGame()
        {
            ChangeGameState(GameState.Playing);
        }

        /// <summary>
        ///     暂停游戏
        /// </summary>
        public void PauseGame()
        {
            ChangeGameState(GameState.Paused);
        }

        /// <summary>
        ///     恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            ChangeGameState(GameState.Playing);
        }

        /// <summary>
        ///     结束游戏
        /// </summary>
        /// <param name="isVictory">是否胜利</param>
        public void EndGame(bool isVictory)
        {
            if (isVictory)
                ChangeGameState(GameState.Victory);
            else
                ChangeGameState(GameState.GameOver);
        }

        /// <summary>
        ///     重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            // 重新加载当前关卡
            LoadCurrentLevel();
            ChangeGameState(GameState.Playing);
        }

        /// <summary>
        ///     返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            currentLevelIndex = 0;
            ChangeGameState(GameState.MainMenu);
        }

        /// <summary>
        ///     退出游戏
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        #endregion

        #region 场景管理

        /// <summary>
        ///     根据状态加载场景
        /// </summary>
        /// <param name="state">游戏状态</param>
        private void LoadSceneForState(GameState state)
        {
            // 查找与状态关联的场景信息
            var sceneInfo = sceneInfos.Find(s => s.associatedState == state);
            if (sceneInfo != null) LoadScene(sceneInfo.sceneName, sceneInfo.isAdditive);
        }

        /// <summary>
        ///     加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="isAdditive">是否叠加加载</param>
        public void LoadScene(string sceneName, bool isAdditive = false)
        {
            if (IsSceneLoading) return;

            IsSceneLoading = true;
            ChangeGameState(GameState.Loading);

            // 广播场景加载开始事件
            OnSceneLoadStarted?.Invoke(sceneName);
            GameEventSystem.Broadcast(new GameEvents.SceneLoadStartedEvent(sceneName));

            // 加载场景
            if (isAdditive)
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                // 如果不是叠加加载，先卸载所有已加载的场景（除了持久场景）
                UnloadAllLoadedScenes();
                SceneManager.LoadSceneAsync(sceneName);
            }

            // 记录加载的场景
            if (!loadedScenes.Contains(sceneName)) loadedScenes.Add(sceneName);
        }

        /// <summary>
        ///     卸载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void UnloadScene(string sceneName)
        {
            if (loadedScenes.Contains(sceneName))
            {
                SceneManager.UnloadSceneAsync(sceneName);
                loadedScenes.Remove(sceneName);
            }
        }

        /// <summary>
        ///     卸载所有已加载的场景（除了持久场景）
        /// </summary>
        private void UnloadAllLoadedScenes()
        {
            foreach (var sceneName in new List<string>(loadedScenes)) UnloadScene(sceneName);
        }

        /// <summary>
        ///     加载当前关卡
        /// </summary>
        public void LoadCurrentLevel()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < sceneInfos.Count)
            {
                var levelInfo = sceneInfos[currentLevelIndex];
                LoadScene(levelInfo.sceneName, levelInfo.isAdditive);
            }
        }

        /// <summary>
        ///     加载下一关
        /// </summary>
        public void LoadNextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= sceneInfos.Count)
            {
                // 所有关卡完成
                GameEventSystem.Broadcast(new GameEvents.LevelCompletedEvent(currentLevelIndex - 1, true));
                ReturnToMainMenu();
            }
            else
            {
                LoadCurrentLevel();
            }
        }

        /// <summary>
        ///     加载指定关卡
        /// </summary>
        /// <param name="levelIndex">关卡索引</param>
        public void LoadLevel(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < sceneInfos.Count)
            {
                currentLevelIndex = levelIndex;
                LoadCurrentLevel();
            }
        }

        #endregion

        #region Scene Callbacks

        /// <summary>
        ///     场景加载完成回调
        /// </summary>
        /// <param name="scene">加载的场景</param>
        /// <param name="mode">加载模式</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            IsSceneLoading = false;

            // 广播场景加载完成事件
            OnSceneLoadCompleted?.Invoke(scene.name);
            GameEventSystem.Broadcast(new GameEvents.SceneLoadCompletedEvent(scene.name));

            // 根据当前状态更新游戏状态
            if (CurrentState == GameState.Loading)
            {
                // 查找加载场景对应的状态
                var sceneInfo = sceneInfos.Find(s => s.sceneName == scene.name);
                if (sceneInfo != null)
                    ChangeGameState(sceneInfo.associatedState);
                else
                    // 如果没有找到对应的状态，默认返回到之前的状态
                    ChangeGameState(previousState);
            }
        }

        /// <summary>
        ///     场景卸载回调
        /// </summary>
        /// <param name="scene">卸载的场景</param>
        private void OnSceneUnloaded(Scene scene)
        {
            // 广播场景卸载事件
            GameEventSystem.Broadcast(new GameEvents.SceneUnloadedEvent(scene.name));
        }

        #endregion

        #region 事件处理

        /// <summary>
        ///     处理暂停游戏请求
        /// </summary>
        private void OnPauseGameRequested(GameEvents.PauseGameEvent evt)
        {
            PauseGame();
        }

        /// <summary>
        ///     处理恢复游戏请求
        /// </summary>
        private void OnResumeGameRequested(GameEvents.ResumeGameEvent evt)
        {
            ResumeGame();
        }

        /// <summary>
        ///     处理重新开始游戏请求
        /// </summary>
        private void OnRestartGameRequested(GameEvents.RestartGameEvent evt)
        {
            RestartGame();
        }

        /// <summary>
        ///     处理返回主菜单请求
        /// </summary>
        private void OnReturnToMainMenuRequested(GameEvents.ReturnToMainMenuEvent evt)
        {
            ReturnToMainMenu();
        }

        /// <summary>
        ///     处理退出游戏请求
        /// </summary>
        private void OnQuitGameRequested(GameEvents.QuitGameEvent evt)
        {
            QuitGame();
        }

        /// <summary>
        ///     处理加载关卡请求
        /// </summary>
        private void OnLoadLevelRequested(GameEvents.LoadLevelEvent evt)
        {
            LoadLevel(evt.levelIndex);
        }

        /// <summary>
        ///     处理关卡完成事件
        /// </summary>
        private void OnLevelCompleted(GameEvents.LevelCompletedEvent evt)
        {
            if (evt.isSuccess)
            {
                // 如果成功完成当前关卡，加载下一关
                if (evt.levelIndex == currentLevelIndex) LoadNextLevel();
            }
            else
            {
                // 如果失败，可以选择重新加载当前关卡或返回主菜单
                // 这里默认重新加载当前关卡
                RestartGame();
            }
        }

        #endregion
    }
}