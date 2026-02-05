using Config;
using GameSystem.GameScene.MainMenu.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem.GameScene.MainMenu
{
    /// <summary>
    ///     场景管理器接口，定义了场景需要实现的基本功能
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        ///     场景名称
        /// </summary>
        string SceneName { get; }

        /// <summary>
        ///     场景是否已完成
        /// </summary>
        bool IsSceneCompleted { get; }

        /// <summary>
        ///     场景是否成功完成
        /// </summary>
        bool IsSceneSuccessful { get; }

        /// <summary>
        ///     场景初始化，在场景加载完成后调用
        /// </summary>
        void InitializeScene();

        /// <summary>
        ///     场景清理，在场景卸载前调用
        /// </summary>
        void CleanupScene();

        /// <summary>
        ///     场景暂停，当游戏暂停时调用
        /// </summary>
        void PauseScene();

        /// <summary>
        ///     场景恢复，当游戏从暂停状态恢复时调用
        /// </summary>
        void ResumeScene();
    }

    /// <summary>
    ///     基础场景管理器，实现了ISceneManager接口的通用功能
    /// </summary>
    public abstract class BaseSceneManager : MonoBehaviour, ISceneManager
    {
        [Header("场景设置")] [SerializeField] protected string sceneName;

        [Header("场景状态")] [SerializeField] protected bool isSceneCompleted;

        [SerializeField] protected bool isSceneSuccessful;

        protected virtual void Awake()
        {
            // 如果场景名称为空，使用当前场景的名称
            if (string.IsNullOrEmpty(sceneName)) sceneName = SceneManager.GetActiveScene().name;
        }

        protected virtual void Start()
        {
            // 初始化场景
            InitializeScene();

            // 订阅游戏流管理器事件
            GameFlowManager.OnGameStateChanged += OnGameStateChanged;
        }

        protected virtual void OnDestroy()
        {
            // 清理场景
            CleanupScene();

            // 取消订阅游戏流管理器事件
            GameFlowManager.OnGameStateChanged -= OnGameStateChanged;
        }

        public string SceneName => sceneName;

        public bool IsSceneCompleted => isSceneCompleted;
        public bool IsSceneSuccessful => isSceneSuccessful;

        // 以下方法由子类实现
        public abstract void InitializeScene();
        public abstract void CleanupScene();

        public virtual void PauseScene()
        {
            Time.timeScale = 0f;
            
        }

        public virtual void ResumeScene()
        {
            Time.timeScale = 1f;
        }

        /// <summary>
        ///     游戏状态变化处理
        /// </summary>
        /// <param name="oldState">旧状态</param>
        /// <param name="newState">新状态</param>
        protected virtual void OnGameStateChanged(SceneInfo oldState,
            SceneInfo newState)
        {
            switch (newState.state)
            {
                case GameState.Paused:
                    PauseScene();
                    break;

                case GameState.Playing:
                    if (oldState.state == GameState.Paused) ResumeScene();
                    break;
            }
        }
    }
}