using System;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
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
        public static GameModeSelect Instance { get; set; }

        [Tooltip("地图")] public static MapSelectInfo Map { get; set; }

        [Tooltip("玩家类型")] public static List<CharacterType> PlayTypes;

        [Tooltip("玩家名称")] public static List<string> PlayerNames;

        [Tooltip("玩家ID")] public static List<int> PlayerIds;
        
        [Tooltip("游戏类型")] public static GameModeType CurrentModeType { get; set; }
        
        [Tooltip("玩家数量")] public static int PlayerCount { get; set; }

        [Tooltip("NPC数量")] public static int EnemyCount { get; set; }

        [Tooltip("玩家控制配置")] public static List<CharacterBaseInfo> CharacterBaseInfos = new List<CharacterBaseInfo>();

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

        public void StartGame()
        {
            if (Map == null)
            {
                Debug.LogError("请选择地图");
            }

            if (EnemyCount + PlayerCount > 4 || EnemyCount < 0 || PlayerCount < 1)
            {
                Debug.LogError("玩家数量或NPC数量错误");
            }

            if (GameFlowManager.Instance == null)
            {
                Debug.LogError("GameFlowManager未初始化");
            }

            if (CharacterBaseInfos.Count != PlayerCount + EnemyCount)
            {
                Debug.LogError("玩家数量与角色信息数量不匹配");
            }
            
            Debug.Log($"[System] 开始游戏！\n加载的地图名称：{Map.mapName}\n加载的场景名称: {Map.mapSceneName}\n 玩家数量：{PlayerCount}\n NPC数量：{EnemyCount}");
            
            
            var sceneInfo = GameFlowManager.Instance.Find(Map.mapSceneName);
            if (sceneInfo == null)
            {
                Debug.LogError("场景信息未找到");
                return;
            }
            GameFlowManager.Instance.ChangeGameState(sceneInfo);
        }
        
        

        public void Clear()
        {
            Map = null;
            PlayTypes = null;
            PlayerNames = null;
            PlayerIds = null;
            CurrentModeType = GameModeType.Offline;
            PlayerCount = 0;
            EnemyCount = 0;
            CharacterBaseInfos.Clear();
        }
        
        public void SetMap(MapSelectInfo map)
        {
            Map = map;
            CharacterBaseInfos.Clear();
        }
        
        public void SetGameMode(GameModeType modeType, int playerCount, int npcCount)
        {
            CurrentModeType = modeType;
            PlayerCount = playerCount;
            EnemyCount = npcCount;
        }
    }

    /// <summary>
    /// 存放角色基础信息
    /// 
    /// </summary>
    public class CharacterBaseInfo
    {
        public CharacterType CharacterType;
        public string CharacterName;
        public string CharacterId;
        public PlayerControlConfig CharacterControlConfig;
        public int Index;
        public Vector3 Spawn;
        public float Angle;

        public CharacterBaseInfo()
        {
        }

        public CharacterBaseInfo(CharacterType characterType, string characterName, string characterId, PlayerControlConfig characterControlConfig)
        {
            CharacterType = characterType;
            CharacterName = characterName;
            CharacterId = characterId;
            CharacterControlConfig = characterControlConfig;
        }

        public CharacterBaseInfo(string typeName, string characterName, string characterId,
            PlayerControlConfig characterControlConfig, int idx, Vector3 spawn, float angle)
        {
            CharacterType = CharacterProper.ParseCharacterType(typeName);
            CharacterName = characterName;
            CharacterId = characterId;
            CharacterControlConfig = characterControlConfig;
            Index = idx;
            Spawn = spawn;
            Angle = angle;
        }
    }
}