using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.EventSystem;
using GameSystem.GameScene.MessageScene;
using GameSystem.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem.GameScene.MainMenu
{
    /// <summary>
    /// 增强版游戏流管理器，负责管理全局游戏状态和场景转换
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        #region 变量定义

        [Header("场景配置")] [SerializeField] private List<SceneInfo> sceneInfos = new();

        // 当前游戏状态
        [SerializeField] private SceneInfo _currentState;

        public SceneInfo CurrentState
        {
            get => _currentState;
            private set => _currentState = value;
        }

        [SerializeField] [Tooltip("之前的状态，用于状态恢复")]
        private SceneInfo _previousState;

        [SerializeField] [Tooltip("正在加载的状态")] private SceneInfo _currentScene;

        [Header("场景管理")] [Tooltip("已加载的场景列表（使用HashSet提高查找性能）")] [SerializeField]
        private HashSet<string> _loadedScenes = new();

        [SerializeField] [Tooltip("需要持久化的场景列表")]
        private HashSet<string> _persistentScenes = new();

        [Tooltip("场景队列")] [SerializeField] private Queue<SceneInfo> _sceneQueue = new();

        /// 场景加载状态
        public bool IsSceneLoading { get; private set; }

        /// 场景加载进度 (0-1)
        public float SceneLoadProgress { get; private set; }

        [Header("加载设置")] [SerializeField] [Tooltip("最小加载时间")]
        public float minLoadTime = 1f; // 最小加载时间(秒)

        #region 事件

        // 游戏状态变化事件
        public static event Action<SceneInfo, SceneInfo> OnGameStateChanged; // (旧状态, 新状态)

        // 场景加载事件
        public static event Action<string> OnSceneLoadStarted; // 开始加载场景
        public static event Action<string, float> OnSceneLoadProgress; // 场景加载进度 (场景名称, 进度0-1)
        public static event Action<string> OnSceneLoadCompleted; // 结束加载场景

        #endregion

        #endregion


        #region 单例模式

        public static GameFlowManager Instance { get; private set; }

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
                UnregisterEventListeners();
                Destroy(gameObject);
            }
        }

        #endregion


        #region 清理现场

        private void OnDestroy()
        {
            // 取消事件监听
            UnregisterEventListeners();

            // 取消场景加载事件
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        #endregion

        /// <summary>
        /// 寻找游戏状态，仅限唯一状态
        /// </summary>
        /// <param name="state">需要寻找的状态</param>
        /// <returns></returns>
        public SceneInfo FindState(GameState state)
        {
            return sceneInfos.Find(x => x.state == state);
        }

        /// <summary>
        /// 根据场景名称查询游戏状态
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>游戏状态</returns>
        public SceneInfo FindState(string sceneName)
        {
            return sceneInfos.Find(x => x.sceneName == sceneName);
        }


        #region 游戏配置

        public bool IsGameActive { get; private set; }

        #endregion


        #region 初始化

        private void Initialize()
        {
            // 订阅场景加载事件
            SceneManager.sceneLoaded += OnSceneLoaded; // 场景加载事件
            SceneManager.sceneUnloaded += OnSceneUnloaded; // 场景卸载事件

            IsSceneLoading = false;

            // 加载场景信息配置
            sceneInfos.Clear();
            sceneInfos = Resources.Load<SceneInfoConfig>("BaseConfig/SceneInfoConfig").sceneInfos;

            if (sceneInfos == null)
            {
                Debug.LogError("场景信息配置为空");
                GlobalMessageManager.Instance.SendMessage("场景信息配置为空");
                return;
            }

            // 设置初始状态

            var startScene = sceneInfos.FindAll(x => x.state == GameState.MainMenu);
            if (startScene.Count > 1) Debug.LogError("存在多个mainMenu场景，请检查配置");

            _currentScene = sceneInfos.Find(x => x.state == GameState.Loading);

            // 设置初始状态
            _previousState = startScene[0];

            CurrentState = startScene[0];
            /*// 设置初始状态
            ChangeGameState(startScene[0]);*/
            // 加载启动时需要加载的场景
            LoadStartupScenes();
            // 注册事件监听
            RegisterEventListeners();
        }


        /// <summary>
        /// 加载启动时需要加载的场景
        /// </summary>
        private void LoadStartupScenes()
        {
            if (sceneInfos == null)
            {
                Debug.LogError("SceneInfoConfig未设置！");
                return;
            }

            foreach (var sceneInfo in sceneInfos)
                if (sceneInfo.loadOnStartup)
                {
                    if (sceneInfo != CurrentState) LoadScene(sceneInfo);
                    //_sceneQueue.Enqueue(sceneInfo);
                    // 如果是持久化场景，添加到持久化场景列表
                    if (sceneInfo.isPersistent) _persistentScenes.Add(sceneInfo.sceneName);
                }
        }


        private void LoadScene()
        {
            while (_sceneQueue.Count > 0)
            {
                var sceneInfo = _sceneQueue.Dequeue();
                LoadScene(sceneInfo);
            }
        }

        private void RegisterEventListeners()
        {
            // 监听游戏相关事件
            GameEventSystem.AddListener<GameEvents.PauseGameEvent>(OnPauseGameRequested);
            GameEventSystem.AddListener<GameEvents.ResumeGameEvent>(OnResumeGameRequested);
            GameEventSystem.AddListener<GameEvents.RestartGameEvent>(OnRestartGameRequested);
            GameEventSystem.AddListener<GameEvents.ReturnToMainMenuEvent>(OnReturnToMainMenuRequested);
            GameEventSystem.AddListener<GameEvents.QuitGameEvent>(OnQuitGameRequested);
        }

        private void UnregisterEventListeners()
        {
            // 取消事件监听
            GameEventSystem.RemoveListener<GameEvents.PauseGameEvent>(OnPauseGameRequested);
            GameEventSystem.RemoveListener<GameEvents.ResumeGameEvent>(OnResumeGameRequested);
            GameEventSystem.RemoveListener<GameEvents.RestartGameEvent>(OnRestartGameRequested);
            GameEventSystem.RemoveListener<GameEvents.ReturnToMainMenuEvent>(OnReturnToMainMenuRequested);
            GameEventSystem.RemoveListener<GameEvents.QuitGameEvent>(OnQuitGameRequested);
        }

        #endregion

        #region Update

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // 处理ESC键
            if (Input.GetKeyDown(KeyCode.Escape))
                switch (CurrentState.state)
                {
                    case GameState.Playing:
                        PauseGame();
                        break;
                    case GameState.Paused:
                        ResumeGame();
                        break;
                    case GameState.Settings:
                        // 从设置界面返回到之前的界面
                        ChangeGameState(_previousState);
                        break;
                }
        }

        #endregion

        #region 游戏状态管理

        /// <summary>
        /// 改变游戏状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void ChangeGameState(SceneInfo newState)
        {
            // 空值检查
            if (newState == null)
            {
                Debug.LogError("新状态为空，无法改变游戏状态");
                return;
            }

            if (CurrentState == newState) return;

            var oldState = CurrentState;
            _previousState = CurrentState;
            CurrentState = newState;

            // 根据状态执行相应操作
            switch (newState.state)
            {
                case GameState.MainMenu:
                    UnloadAllLoadedScenes();
                    break;
                case GameState.Loading:
                    Time.timeScale = 1f;
                    IsGameActive = false;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    IsGameActive = true;
                    // 如果从非暂停状态进入，加载当前关卡
                    if (oldState.state != GameState.Paused && oldState.state != GameState.Playing)
                        StartCoroutine(LoadSceneWithTransitionCoroutine(newState));
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
        /// 卸载所有已加载的场景（除了指定的场景和持久化场景）
        /// </summary>
        /// <param name="sceneNameToKeep">要保留的场景名称</param>
        private void UnloadAllLoadedScenesExcept(string sceneNameToKeep = null)
        {
            foreach (var sceneName in _loadedScenes)
                // 保留指定的场景和持久化场景
                if (sceneName != sceneNameToKeep && !_persistentScenes.Contains(sceneName))
                    UnloadScene(sceneName);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            ChangeGameState(FindState(GameState.Paused));
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            ChangeGameState(FindState(GameState.Playing));
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        /// <param name="isVictory">是否胜利</param>
        public void EndGame(bool isVictory)
        {
            if (isVictory)
                ChangeGameState(FindState(GameState.Victory));
            else
                ChangeGameState(FindState(GameState.GameOver));
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            // 重新加载当前关卡
            ChangeGameState(CurrentState);
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            ChangeGameState(FindState(GameState.MainMenu));
            MainUIController.Reset();
            MainUIManager.Instance.ShowPanel(PanelSymbols.BgPanel, true);
            MainUIManager.Instance.ShowPanel(PanelSymbols.MainPanel);
        }

        /// <summary>
        /// 退出游戏
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

        public void ReLoadCurrentScene()
        {
            if (CurrentState == null)
            {
                Debug.LogError("当前状态为空，无法重新加载场景");
                return;
            }

            // 获取当前场景名称
            var currentSceneName = CurrentState.sceneName;

            if (string.IsNullOrEmpty(currentSceneName))
            {
                Debug.LogError("当前场景名称为空，无法重新加载");
                return;
            }

            if (CurrentState.state == GameState.Playing)
                LoadScene(CurrentState, true);
            else
                LoadScene(CurrentState);
        }


        /// <summary>
        /// 根据SceneInfo加载场景
        /// </summary>
        /// <param name="sceneInfo">场景信息</param>
        /// <param name="isUseTransition">是否使用过渡场景</param>
        private void LoadScene(SceneInfo sceneInfo, bool isUseTransition = false)
        {
            IsSceneLoading = true;
            SceneLoadProgress = 0f;

            if (isUseTransition)
                StartCoroutine(LoadSceneWithTransitionCoroutine(sceneInfo));
            else
                StartCoroutine(LoadSceneAsyncCoroutine(sceneInfo.sceneName, sceneInfo.isAdditive));
        }

        /// <summary>
        /// 加载场景（根据SceneInfo配置）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="isUseTransition">是否使用过渡场景</param>
        public void LoadScene(string sceneName, bool isUseTransition = false)
        {
            if (sceneInfos == null)
            {
                Debug.LogError("SceneInfoConfig未设置！");
                return;
            }

            // 从配置中查找场景信息
            var sceneInfo = sceneInfos.Find(s => s.sceneName == sceneName);
            if (sceneInfo == null)
            {
                Debug.LogError($"场景 {sceneName} 未在SceneInfoConfig中配置！");
                return;
            }

            LoadScene(sceneInfo, isUseTransition);
        }

        /// <summary>
        /// 带过渡场景的加载协程
        /// </summary>
        private IEnumerator LoadSceneWithTransitionCoroutine(SceneInfo sceneInfo)
        {
            var loadStartTime = Time.time;
            var sceneName = sceneInfo.sceneName;

            // 广播场景加载开始事件
            GameEventSystem.Broadcast(new GameEvents.SceneLoadStartedEvent(sceneName));

            // 先加载过渡场景
            var loadingSceneOperation = SceneManager.LoadSceneAsync(_currentScene.sceneName, LoadSceneMode.Additive);

            if (loadingSceneOperation == null)
            {
                Debug.LogError("过渡场景加载失败！");
                yield break;
            }

            // 等待过渡场景加载完成
            while (!loadingSceneOperation.isDone) yield return null;

            OnSceneLoadStarted?.Invoke(sceneName);
            // 卸载当前场景（除了过渡场景）
            UnloadAllLoadedScenesExcept(_currentScene.sceneName);

            // 开始异步加载目标场景
            var targetSceneOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (targetSceneOperation == null)
            {
                Debug.LogError("目标场景 " + sceneName + " 加载失败！");
                yield break;
            }

            // 禁用目标场景的激活，直到加载完成
            targetSceneOperation.allowSceneActivation = false;

            // 等待目标场景加载完成
            while (targetSceneOperation.progress < 0.9f)
            {
                // 更新加载进度
                SceneLoadProgress = targetSceneOperation.progress;

                // 广播加载进度事件
                OnSceneLoadProgress?.Invoke(sceneName, SceneLoadProgress);

                yield return null;
            }

            // 确保至少加载minLoadTime秒
            var elapsedTime = Time.time - loadStartTime;
            if (elapsedTime < minLoadTime)
            {
                var remainingTime = minLoadTime - elapsedTime;

                // 在剩余时间内更新进度到100%
                while (remainingTime > 0)
                {
                    remainingTime -= Time.deltaTime;
                    SceneLoadProgress = 1f - remainingTime / minLoadTime;
                    OnSceneLoadProgress?.Invoke(sceneName, SceneLoadProgress);
                    yield return null;
                }
            }

            // 确保进度为100%
            SceneLoadProgress = 1f;
            OnSceneLoadProgress?.Invoke(sceneName, SceneLoadProgress);

            // 记录加载的场景
            if (!_loadedScenes.Contains(sceneName)) _loadedScenes.Add(sceneName);

            // 广播场景加载完成事件
            OnSceneLoadCompleted?.Invoke(sceneName);
            GameEventSystem.Broadcast(new GameEvents.SceneLoadCompletedEvent(sceneName));

            targetSceneOperation.allowSceneActivation = true;

            // 等待目标场景激活完成
            while (!targetSceneOperation.isDone) yield return null;

            // 卸载过渡场景
            SceneManager.UnloadSceneAsync(_currentScene.sceneName);
            IsSceneLoading = false;
        }

        /// <summary>
        /// 异步加载场景协程
        /// </summary>
        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, bool isAdditive)
        {
            AsyncOperation asyncOperation;

            // 加载场景
            if (isAdditive)
            {
                asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                // 如果不是叠加加载，先卸载所有已加载的场景（除了持久场景）
                UnloadAllLoadedScenes();
                asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            }

            if (asyncOperation == null)
            {
                Debug.LogError("场景 " + sceneName + " 加载失败！");
                yield break;
            }

            // 记录加载的场景
            if (!_loadedScenes.Contains(sceneName)) _loadedScenes.Add(sceneName);

            // 等待场景加载完成
            while (!asyncOperation.isDone) yield return null;

            IsSceneLoading = false;
        }


        /// <summary>
        /// 卸载所有已加载的场景（除了持久化场景）
        /// </summary>
        private void UnloadAllLoadedScenes()
        {
            List<string> scenesToUnload = new();
            foreach (var sceneName in _loadedScenes)
                // 不卸载持久化场景
                if (!_persistentScenes.Contains(sceneName))
                    scenesToUnload.Add(sceneName);
            foreach (var s in scenesToUnload)
                UnloadScene(s);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void UnloadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("场景名称为空，无法卸载");
                return;
            }

            if (_loadedScenes.Contains(sceneName))
            {
                SceneManager.UnloadSceneAsync(sceneName);
                _loadedScenes.Remove(sceneName);
            }
        }

        #endregion

        #region 场景回调

        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        /// <param name="scene">加载的场景</param>
        /// <param name="mode">加载模式</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            IsSceneLoading = false;

            // 广播场景加载完成事件
            OnSceneLoadCompleted?.Invoke(scene.name);
            GameEventSystem.Broadcast(new GameEvents.SceneLoadCompletedEvent(scene.name));
        }

        /// <summary>
        /// 场景卸载回调
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
        /// 处理暂停游戏请求
        /// </summary>
        private void OnPauseGameRequested(GameEvents.PauseGameEvent evt)
        {
            PauseGame();
        }

        /// <summary>
        /// 处理恢复游戏请求
        /// </summary>
        private void OnResumeGameRequested(GameEvents.ResumeGameEvent evt)
        {
            ResumeGame();
        }

        /// <summary>
        /// 处理重新开始游戏请求
        /// </summary>
        private void OnRestartGameRequested(GameEvents.RestartGameEvent evt)
        {
            RestartGame();
        }

        /// <summary>
        /// 处理返回主菜单请求
        /// </summary>
        private void OnReturnToMainMenuRequested(GameEvents.ReturnToMainMenuEvent evt)
        {
            ReturnToMainMenu();
        }

        /// <summary>
        /// 处理退出游戏请求
        /// </summary>
        private void OnQuitGameRequested(GameEvents.QuitGameEvent evt)
        {
            QuitGame();
        }

        #endregion
    }
}