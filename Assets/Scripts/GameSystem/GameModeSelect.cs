using UnityEngine;

namespace GameSystem
{
    //游戏状态
    [System.Serializable]
    public enum GameState
    {
        MainMenu,//主菜单
        InGame,//游戏中
        GameOver//游戏结束
    }

    //游戏类型
    [System.Serializable]
    public enum GameType
    {
        Online,//在线
        Offline//离线
    }
    
    
    
    
    
    public class GameModeSelect : MonoBehaviour
    {
        [Tooltip("游戏状态")]
        public static GameState CurrentState { get; private set; }
        [Tooltip("游戏类型")]
        public static GameType CurrentType { get; private set; }
        [Tooltip("是否启用NPC")]
        public static bool IsEnableNPC { get; private set; }
        [Tooltip("玩家数量")]
        public static int PlayerCount { get; private set; }
        [Tooltip("NPC数量")]
        public static int NPCCount { get; private set; }
        
        
        public static GameModeSelect Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void SetGameMode(GameType type, GameState state, int playerCount, int npcCount)
        {
            CurrentType = type;
            CurrentState = state;
            PlayerCount = playerCount;
            NPCCount = npcCount;  
        }
        
        // 判断是否为离线单人模式
        public bool IsOffOnlineSinglePlayer()
        {
            return CurrentType == GameType.Offline && PlayerCount == 1;
        }

        // 判断是否为离线多人模式
        public bool IsOfflineMultiPlayer()
        {
            return CurrentType == GameType.Offline && PlayerCount > 1;
        }

        // 判断是否为在线模式
        public bool IsOnlineMode()
        {
            return CurrentType == GameType.Online;
        }

        // 判断是否为单人模式（包括在线和离线）
        public bool IsSinglePlayer()
        {
            return PlayerCount == 1;
        }

        // 判断是否为多人模式（包括在线和离线）
        public bool IsMultiPlayer()
        {
            return PlayerCount > 1;
        }
        
        
    }
        
}