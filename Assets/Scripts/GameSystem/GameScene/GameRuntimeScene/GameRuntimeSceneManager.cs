using System.Collections.Generic;
using GameSystem.EventSystem;
using player;
using UnityEngine;

namespace GameSystem.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameRuntimeSceneManager : MonoBehaviour
    {
        public static GameRuntimeSceneManager Instance { get; private set; }
        
        [Header("初始组件 索引0为单人，1-4为多人")]
        [Tooltip("玩家预制体")]
        public GameObject players;
        [Tooltip("玩家数量")]
        public int playerCount;
        [Tooltip("NPC数量")]
        public int npcCount;
        [Tooltip("玩家类型")]
        public List<PlayType> playTypes;
        [Tooltip("玩家名称")] 
        public List<string> playerNames;
        [Tooltip("玩家ID")]
        public List<int> playerIds;
        [Tooltip("NPC预制体")]
        public List<GameObject> npcs;
        [Tooltip("玩家出生点")]
        public List<Transform> spawns;
        [Tooltip("玩家状态HUD")] 
        public List<GameObject> huds;


        [Header("初始参数")]
        [Tooltip("是否处于Debug模式")]
        public bool isDebug = false;
        [Tooltip("加载第几个角色")]
        public int loadPlayer = 0;
        [Tooltip("当前玩家数")]
        public int currentPlayerCount = 0;
        [Tooltip("当前NPC数")]
        public int currentNPCCount = 0;
        
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            InitVariable();
            if (isDebug)
            { 
                LoadPlayer(loadPlayer);
            }
            else
            {
                if (GameModeSelect.Instance == null)
                {
                    Debug.LogError("在GameRuntimeSceneManager初始化过程中GameModeSelect为空");
                }
                else
                {
                    currentPlayerCount = 1;
                    InitGame();
                }
            }
            
        }

        private void InitVariable()
        {
            if (spawns.Count == 5 || huds.Count == 5)
            {
                foreach (var vaSpawn in spawns)
                {
                    if (vaSpawn == null)
                    {
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家出生点为空");
                    }
                }
                foreach (var vaHud in huds)
                {
                    if(vaHud == null){
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家状态HUD为空");                      
                    }
                }
            }
            else
            {
                Debug.LogError("在GameRuntimeSceneManager初始化过程中出现出生点数量或HUD数量不足");
            }
        }
        
        private void ValidateReference(object obj, string name)
        {
            if (obj == null) Debug.LogError($"{name}为空");
        }

        private void LoadPlayer(int index)
        {
            //实例化游戏对象
            GameObject player = Instantiate(Instance.players, Instance.spawns[index].position, Instance.spawns[index].rotation);
            //创建玩家控制器
            PlayerController playerController = player.AddComponent<PlayerController>();
            //获取HUD控制器
            PlayerStateHUD playerStateHUD = Instance.huds[index].GetComponent<PlayerStateHUD>();
            if (playerStateHUD == null)
            {
                Debug.LogError("在GameRuntimeSceneManager初始化过程中playerStateHUD为空");
            }
            //初始化玩家控制器
            playerController.PlayerControllerInit(Instance.playerNames[index], Instance.playerIds[index], Instance.playTypes[index], playerStateHUD);
            //启用HUD
            Instance.huds[index].SetActive(true);
            //启用玩家
            player.SetActive(true);
        }
        
        
        public void InitGame()
        {
            currentPlayerCount = playerCount;
            currentNPCCount = npcCount;
            // TODO: 初始化游戏场景
            if (GameModeSelect.Instance.IsOnlineMode())
            {
                // TODO: 初始化在线游戏场景
                
                
            }
            else
            {
                // TODO: 初始化离线游戏场景
                if (GameModeSelect.Instance.IsSinglePlayer())
                {
                    LoadPlayer(0);
                }
                else
                {
                    // TODO: 初始化多人游戏场景
                    for (int i = 1; i <= Instance.playerCount; i++)
                    {
                        LoadPlayer(i);
                    }
                }
            }
        }
        public static void Load()
        {
            // TODO: 加载游戏场景
        }
        
        public static void Unload()
        {
            // TODO: 卸载游戏场景
        }

        public void OnGameCharacterDie(PlayerDieEvent evt)
        {
            // TODO: 处理游戏角色死亡事件
           if (evt.DieId < 0)
           {
               npcCount--;
           }
           else
           {
               currentPlayerCount--;
           }

           if (currentPlayerCount > 1)
           {
               return;
           }
           else if (currentPlayerCount == 1 && currentNPCCount == 0)
           {
               print("游戏结束");
               GameEventSystem.Broadcast(new GameOverEvent()
               {
                   isWin = true
               });
           }
           else if(currentPlayerCount == 0 && currentNPCCount != 0)
           {
               //NPC赢
               print("游戏结束");
               GameEventSystem.Broadcast(new GameOverEvent()
               {
                   isWin = false
               });
           }
           
           
           
        }

           
    }
}