using System.Collections.Generic;
using player;
using UnityEngine;

namespace GameSystem
{
    //游戏状态


    //游戏类型
    [System.Serializable]
    public enum GameType
    {
        Online,//在线
        Offline//离线
    }
    
    public enum GameMode
    {
        PVP,
        PVE
    }
    
    
    
    
    
    public class GameModeSelect : MonoBehaviour
    {
        public static GameModeSelect Instance { get;  set; }

        [Tooltip("游戏状态")]
        public static GameState CurrentState { get; set; }
        [Tooltip("游戏类型")]
        public static GameType CurrentType { get;set; }
        [Tooltip("是否启用NPC")]
        public static bool IsEnableNPC { get; set; }
        [Tooltip("玩家数量")]
        public static int PlayerCount { get; set; }
        [Tooltip("NPC数量")]
        public static int NPCCount { get; set; }
        [Tooltip("玩家类型")]
        public static List<PlayType> playTypes;
        [Tooltip("玩家名称")] 
        public static List<string> playerNames;
        [Tooltip("玩家ID")]
        public static List<int> playerIds;
        
        

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