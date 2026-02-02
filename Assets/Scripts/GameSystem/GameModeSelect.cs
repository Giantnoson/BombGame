using System;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace GameSystem
{
    //游戏状态


    //游戏类型
    [Serializable]
    public enum GameModeType
    {
        Online, //在线
        Offline, //离线
    }

    public enum GameMode
    {
        PVP,
        PVE
    }


    public class GameModeSelect : MonoBehaviour
    {
        [Tooltip("玩家类型")] public static List<CharacterType> playTypes;

        [Tooltip("玩家名称")] public static List<string> playerNames;

        [Tooltip("玩家ID")] public static List<int> playerIds;

        public static GameModeSelect Instance { get; set; }

        [Tooltip("游戏状态")] public static EnhancedGameFlowManager.GameState CurrentState { get; set; }

        [Tooltip("游戏类型")] public static GameModeType CurrentModeType { get; set; }
        
        [Tooltip("玩家数量")] public static int PlayerCount { get; set; }

        [Tooltip("NPC数量")] public static int NPCCount { get; set; }


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
            }
        }

        
        public void SetGameMode(GameModeType modeType, EnhancedGameFlowManager.GameState state, int playerCount, int npcCount)
        {
            CurrentModeType = modeType;
            CurrentState = state;
            PlayerCount = playerCount;
            NPCCount = npcCount;
        }


    }
}