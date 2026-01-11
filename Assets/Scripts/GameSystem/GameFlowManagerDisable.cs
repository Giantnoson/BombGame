using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace GameSystem
{
    // 游戏状态枚举
    public enum GameStateDisable
    {
        MainMenu,    // 主菜单
        Playing,     // 游戏进行中
        Paused,      // 游戏暂停
        GameOver,    // 游戏结束
        Victory      // 游戏胜利
    }

    public class GameFlowManagerDisable : MonoBehaviour
    {
        public static GameFlowManagerDisable Instance { get; private set; }

        [Header("游戏状态")]
        [SerializeField]
        private GameStateDisable currentStateDisable = GameStateDisable.MainMenu;

        [Header("UI元素")]
        [SerializeField]
        private GameObject pauseMenuUI;
        [SerializeField]
        private GameObject gameOverUI;
        [SerializeField]
        private GameObject victoryUI;
        [SerializeField]
        private TextMeshProUGUI timerText;

        [Header("游戏设置")]
        [SerializeField]
        private float gameDuration = 180f; // 游戏时长（秒）
        [SerializeField]
        private List<string> gameSceneNames = new List<string>(); // 游戏场景名称列表
        [SerializeField]
        private int currentLevelIndex = 0; // 当前关卡索引

        private float gameTimer;
        private bool isGameActive = false;
        private bool isLoadingScene = false; // 场景加载标志

        // 事件
        public static event Action<GameStateDisable> OnGameStateChanged;

        // 属性
        public GameStateDisable CurrentStateDisable => currentStateDisable;
        public float GameTimer => gameTimer;
        public bool IsGameActive => isGameActive;
        public int CurrentLevelIndex => currentLevelIndex;
        public int TotalLevels => gameSceneNames.Count;
        public bool IsLoadingScene => isLoadingScene;

        private void Awake()
        {
            // 单例模式
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 初始化游戏
            InitializeGame();
        }

        private void Update()
        {
            // 更新游戏
            UpdateGame();

            // 检查暂停输入
            HandlePauseInput();
        }

        // 初始化游戏
        private void InitializeGame()
        {
            // 确保所有UI元素初始状态正确
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (victoryUI != null) victoryUI.SetActive(false);

            // 设置初始状态
            ChangeGameState(GameStateDisable.MainMenu);
            
            // 订阅场景加载事件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // 更新游戏
        private void UpdateGame()
        {
            // 如果游戏正在进行中，更新计时器
            if (currentStateDisable == GameStateDisable.Playing && isGameActive)
            {
                gameTimer -= Time.deltaTime;

                // 更新计时器UI
                if (timerText != null)
                {
                    int minutes = Mathf.FloorToInt(gameTimer / 60);
                    int seconds = Mathf.FloorToInt(gameTimer % 60);
                    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }

                // 检查时间是否用完
                if (gameTimer <= 0)
                {
                    EndGame(false); // 时间到，游戏失败
                }
            }
        }

        // 处理暂停输入
        private void HandlePauseInput()
        {
            if (currentStateDisable == GameStateDisable.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            else if (currentStateDisable == GameStateDisable.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
        }

        // 改变游戏状态
        public void ChangeGameState(GameStateDisable newStateDisable)
        {
            if (currentStateDisable == newStateDisable) return;

            GameStateDisable previousStateDisable = currentStateDisable;
            currentStateDisable = newStateDisable;

            // 根据状态执行相应操作
            switch (newStateDisable)
            {
                case GameStateDisable.MainMenu:
                    Time.timeScale = 1f;
                    isGameActive = false;
                    LoadSceneAsync("MainMenu");
                    break;

                case GameStateDisable.Playing:
                    Time.timeScale = 1f;
                    isGameActive = true;
                    if (previousStateDisable != GameStateDisable.Paused)
                    {
                        gameTimer = gameDuration;
                        // 如果从主菜单或其他非游戏状态进入，加载当前关卡
                        if (previousStateDisable != GameStateDisable.Playing && previousStateDisable != GameStateDisable.Paused)
                        {
                            LoadCurrentLevel();
                        }
                    }
                    break;

                case GameStateDisable.Paused:
                    Time.timeScale = 0f;
                    isGameActive = false;
                    if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
                    break;

                case GameStateDisable.GameOver:
                    Time.timeScale = 0f;
                    isGameActive = false;
                    if (gameOverUI != null) gameOverUI.SetActive(true);
                    break;

                case GameStateDisable.Victory:
                    Time.timeScale = 0f;
                    isGameActive = false;
                    if (victoryUI != null) victoryUI.SetActive(true);
                    break;
            }

            // 触发状态变化事件
            OnGameStateChanged?.Invoke(newStateDisable);
        }

        // 开始游戏
        public void StartGame()
        {
            ChangeGameState(GameStateDisable.Playing);
        }

        // 暂停游戏
        public void PauseGame()
        {
            ChangeGameState(GameStateDisable.Paused);
        }

        // 恢复游戏
        public void ResumeGame()
        {
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            ChangeGameState(GameStateDisable.Playing);
        }

        // 结束游戏
        public void EndGame(bool isVictory)
        {
            if (isVictory)
            {
                ChangeGameState(GameStateDisable.Victory);
            }
            else
            {
                ChangeGameState(GameStateDisable.GameOver);
            }
        }

        // 重新开始游戏
        public void RestartGame()
        {
            // 重新加载当前关卡
            LoadCurrentLevel();
            ChangeGameState(GameStateDisable.Playing);
        }

        // 返回主菜单
        public void ReturnToMainMenu()
        {
            ChangeGameState(GameStateDisable.MainMenu);
        }

        // 退出游戏
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        // 场景加载完成回调
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isLoadingScene = false;
            
            // 根据场景名称更新UI引用
            UpdateUIReferences();
        }
        
        // 更新UI引用
        private void UpdateUIReferences()
        {
            // 查找并更新UI元素引用
            if (pauseMenuUI == null)
                pauseMenuUI = GameObject.Find("PauseMenuUI");
                
            if (gameOverUI == null)
                gameOverUI = GameObject.Find("GameOverUI");
                
            if (victoryUI == null)
                victoryUI = GameObject.Find("VictoryUI");
                
            if (timerText == null)
            {
                GameObject timerObj = GameObject.Find("TimerText");
                if (timerObj != null)
                    timerText = timerObj.GetComponent<TextMeshProUGUI>();
            }
            
            // 确保UI初始状态正确
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (victoryUI != null) victoryUI.SetActive(false);
        }
        
        // 加载当前关卡
        public void LoadCurrentLevel()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < gameSceneNames.Count)
            {
                LoadSceneAsync(gameSceneNames[currentLevelIndex]);
            }
        }
        
        // 加载下一关
        public void LoadNextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= gameSceneNames.Count)
            {
                // 所有关卡完成，返回主菜单
                ReturnToMainMenu();
            }
            else
            {
                LoadCurrentLevel();
            }
        }
        
        // 异步加载场景
        private void LoadSceneAsync(string sceneName)
        {
            if (!isLoadingScene)
            {
                isLoadingScene = true;
                SceneManager.LoadSceneAsync(sceneName);
            }
        }
    }
}