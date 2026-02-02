using System;
using System.Collections.Generic;
using Config;
using GameSystem.Character;
using GameSystem.Character.Player;
using GameSystem.EventSystem;
using UnityEngine;

namespace GameSystem.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameRuntimeSceneManager : BaseSceneManager
    {
        [Header("初始组件 索引0为单人，1-4为多人")] [Tooltip("玩家预制体")]
        public GameObject players;

        [Tooltip("玩家数量")] public int playerCount;

        [Tooltip("NPC数量")] public int npcCount;

        [Tooltip("玩家类型")] public List<CharacterType> playTypes;

        [Tooltip("玩家输入控制")] public List<PlayerControlConfig> ControlConfigs;

        [Tooltip("玩家名称")] public List<string> playerNames;

        [Tooltip("玩家ID")] public List<string> playerIds;

        [Tooltip("NPC预制体")] public List<GameObject> npcs;

        [Tooltip("玩家出生点")] public List<Transform> spawns;

        [Tooltip("玩家状态HUD")] public List<GameObject> huds;


        [Header("初始参数")] [Tooltip("是否处于Debug模式")]
        public bool isDebug;

        [Tooltip("加载第几个角色")] public int loadPlayer;

        [Tooltip("当前玩家数")] public int currentPlayerCount;

        [Tooltip("当前NPC数")] public int currentNPCCount;

        public static GameRuntimeSceneManager Instance { get; private set; }


        protected override void Awake()
        {
            base.Awake();
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
        }

        private void OnEnable()
        {
            // 订阅游戏流管理器事件
            EnhancedGameFlowManager.OnGameStateChanged += OnGameStateChanged;
            GameEventSystem.AddListener<CharacterDieEvent>(OnGameCharacterDie);
        }

        private void OnDisable()
        {
            EnhancedGameFlowManager.OnGameStateChanged -= OnGameStateChanged;
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnGameCharacterDie);
        }
        

        public override void InitializeScene()
        {
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
        
        public override void CleanupScene()
        {
            // 清理逻辑
        }

        private void InitVariable()
        {
            if (spawns.Count == 5 || huds.Count == 5|| ControlConfigs.Count == 5)
            {
                foreach (var vaSpawn in spawns)
                    if (vaSpawn == null)
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家出生点为空");

                foreach (var vaHud in huds)
                    if (vaHud == null)
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家状态HUD为空");
                foreach (var controlConfig in ControlConfigs)
                {
                    if (controlConfig == null)
                    {
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家输入控制为空");
                    }
                }
            }
            else
            {
                Debug.LogError("在GameRuntimeSceneManager初始化过程中出现出生点数量/HUD/玩家输入控制数量不足");
            }
        }

        private void LoadPlayer(int index)
        {
            //实例化游戏对象
            var player = Instantiate(Instance.players, Instance.spawns[index].position,
                Instance.spawns[index].rotation);
            //创建玩家控制器
            var playerController = player.AddComponent<PlayerController>();
            //获取HUD控制器
            var playerStateHUD = Instance.huds[index].GetComponent<PlayerStateHUD>();
            if (playerStateHUD == null) Debug.LogError("在GameRuntimeSceneManager初始化过程中playerStateHUD为空");
            //初始化玩家控制器
            if (GameModeSelect.CurrentModeType == GameModeType.OfflinePVP && GameModeSelect.PlayerCount == 2)
            {
                if (index == 1)
                {
                    playerController.PlayerControllerInit(Instance.playerNames[1], Instance.playerIds[1],
                        Instance.playTypes[1], Instance.ControlConfigs[1]);
                }
                else
                {
                    playerController.PlayerControllerInit(Instance.playerNames[3], Instance.playerIds[3],
                        Instance.playTypes[3], Instance.ControlConfigs[3]);
                }
            }
            else
            {
                playerController.PlayerControllerInit(Instance.playerNames[index], Instance.playerIds[index],
                    Instance.playTypes[index], Instance.ControlConfigs[index]);
            }

            var controller = playerController.GetComponent<CharacterMoveController>();
            if (controller == null) Debug.LogError("在GameRuntimeSceneManager初始化过程中PlayerMoveController为空");

            controller.Init(playerController.id);
            playerStateHUD.LoadHUD(playerController.id);
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
            if (GameModeSelect.CurrentModeType == GameModeType.OnlinePVP)
            {
                // TODO: 初始化在线游戏场景
            }
            else
            {
                // TODO: 初始化离线游戏场景
                if (GameModeSelect.CurrentModeType == GameModeType.OfflinePVE)
                    LoadPlayer(0);
                else
                    // TODO: 初始化多人游戏场景
                    for (var i = 1; i <= Instance.playerCount; i++)
                        LoadPlayer(i);
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

        public void OnGameCharacterDie(CharacterDieEvent evt)
        {
            // TODO: 处理游戏角色死亡事件
            if (evt.DieId[0] == 'n')
                npcCount--;
            else
                currentPlayerCount--;

            if (currentPlayerCount > 1)
            {
            }
            else if (currentPlayerCount == 1 && currentNPCCount == 0)
            {
                print("游戏结束");
                CompleteScene(true);
            }
            else if (currentPlayerCount == 0 && currentNPCCount != 0)
            {
                //NPC赢
                print("游戏结束");
                CompleteScene(false);
            }
        }
    }
}