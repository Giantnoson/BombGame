using Config;

namespace GameSystem.GameScene.MainMenu.EventSystem
{
    /// <summary>
    ///     游戏流管理器相关事件
    /// </summary>
    public static class GameEvents
    {
        #region GameState Events

        /// <summary>
        ///     游戏状态变化事件
        /// </summary>
        public class GameStateChangedEvent : GameEvent
        {
            public readonly SceneInfo newState;
            public readonly SceneInfo oldState;

            public GameStateChangedEvent(SceneInfo oldState,
                SceneInfo newState)
            {
                this.oldState = oldState;
                this.newState = newState;
            }
        }

        #endregion

        #region Scene Events

        /// <summary>
        ///     场景加载开始事件
        /// </summary>
        public class SceneLoadStartedEvent : GameEvent
        {
            public readonly string sceneName;

            public SceneLoadStartedEvent(string sceneName)
            {
                this.sceneName = sceneName;
            }
        }

        /// <summary>
        ///     场景加载完成事件
        /// </summary>
        public class SceneLoadCompletedEvent : GameEvent
        {
            public readonly string sceneName;

            public SceneLoadCompletedEvent(string sceneName)
            {
                this.sceneName = sceneName;
            }
        }

        /// <summary>
        ///     场景卸载事件
        /// </summary>
        public class SceneUnloadedEvent : GameEvent
        {
            public readonly string sceneName;

            public SceneUnloadedEvent(string sceneName)
            {
                this.sceneName = sceneName;
            }
        }

        #endregion

        #region 游戏控制事件

        /// <summary>
        ///     暂停游戏事件
        /// </summary>
        public class PauseGameEvent : GameEvent
        {
        }

        /// <summary>
        ///     恢复游戏事件
        /// </summary>
        public class ResumeGameEvent : GameEvent
        {
        }

        /// <summary>
        ///     重新开始游戏事件
        /// </summary>
        public class RestartGameEvent : GameEvent
        {
        }

        /// <summary>
        ///     返回主菜单事件
        /// </summary>
        public class ReturnToMainMenuEvent : GameEvent
        {
        }

        /// <summary>
        ///     退出游戏事件
        /// </summary>
        public class QuitGameEvent : GameEvent
        {
        }

        #endregion
    }
}