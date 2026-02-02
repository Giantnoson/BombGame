using System;
using System.Collections.Generic;
using Config;
using GameSystem.Character;
using GameSystem.Character.Enemy;
using GameSystem.Character.Player;
using GameSystem.EventSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameRuntimeSceneManager : BaseSceneManager
    {
        [Header("初始组件")] [Tooltip("玩家预制体")]
        public GameObject Character;

        [Tooltip("玩家数量")] public int playerCount;

        [Tooltip("NPC数量")] public int npcCount;

        [Tooltip("玩家类型")] public List<CharacterType> playTypes;

        [Tooltip("玩家输入控制")] public List<PlayerControlConfig> ControlConfigs;

        [Tooltip("玩家名称")] public List<string> playerNames;

        [Tooltip("玩家ID")] public List<string> playerIds;
        
        [Tooltip("敌人名称")] public List<string> enemyNames;

        [Tooltip("敌人ID")] public List<string> enemyIds;
        
        [Tooltip("敌人类型")] public List<CharacterType> enemyTypes;
        
        [Tooltip("玩家出生点")] public List<Transform> spawns;

        [Tooltip("玩家状态HUD")] public List<GameObject> huds;


        [Header("初始参数")] [Tooltip("是否处于Debug模式")]
        public bool isDebug = false;

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
                    playerCount = GameModeSelect.PlayerCount;
                    npcCount = GameModeSelect.NPCCount;
                    if (npcCount + playerCount > 4)
                    {
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家数量/敌人数量超过4");
                    }
                    InitGame();
                }
            }
        }
        
        public void InitGame()
        {
            currentPlayerCount = playerCount;
            currentNPCCount = npcCount;
            // TODO: 初始化游戏场景
            if (GameModeSelect.CurrentModeType == GameModeType.Online)
            {
                // TODO: 初始化在线游戏场景
            }
            else
            {
                if (playerCount == 1)
                {
                    LoadPlayer(0);
                    for (int i = 0; i < npcCount; i++)
                    {
                        LoadNPC(i + 2);
                    }
                }
                else if(playerCount == 2)
                {
                    LoadPlayer(1);
                    LoadPlayer(3);
                    for (int i = 1; i <= npcCount; i++)
                    {
                        LoadNPC(i * 2);
                    }
                }
                else if(playerCount == 3)
                {
                    LoadPlayer(1);
                    LoadPlayer(2);
                    LoadPlayer(3);
                    if (npcCount == 1)
                    {
                        LoadNPC(4);
                    }
                }
                else
                {
                    LoadPlayer(1);
                    LoadPlayer(2);
                    LoadPlayer(3);
                    LoadPlayer(4);
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
            var player = Instantiate(Character, spawns[index].position,
                spawns[index].rotation);
            //获取HUD控制器
            huds[index].SetActive(true);
            var playerStateHUD = Instance.huds[index].GetComponent<PlayerStateHUD>();
            if (playerStateHUD == null) Debug.LogError("在GameRuntimeSceneManager初始化过程中playerStateHUD为空");
            playerStateHUD.LoadHUD(playerIds[index]);
            
            //创建玩家控制器
            var playerController = player.AddComponent<PlayerController>();
            if (index != 0)
                playerController.DisableCamera();
            playerController.PlayerControllerInit(playerNames[index], playerIds[index],
                playTypes[index], ControlConfigs[index]);
            
            //创建玩家移动控制器
            var controller = playerController.AddComponent<CharacterMoveController>();
            controller.Init(playerController.id);
            //启用预制体
            player.name = $"player{index}";
            player.tag = nameof(ObjectType.Player);
            player.SetActive(true);
        }

        private void LoadNPC(int index)
        {
            //实例化游戏对象
            var enemy = Instantiate(Instance.Character, Instance.spawns[index].position,
                Instance.spawns[index].rotation);
            //创建HUD
            var playerStateHUD = Instance.huds[index].GetComponent<PlayerStateHUD>();
            if (playerStateHUD == null) Debug.LogError("在GameRuntimeSceneManager初始化过程中playerStateHUD为空");
            playerStateHUD.LoadHUD(enemyIds[index]);
            huds[index].SetActive(true);
            //初始化敌人移动控制器
            var moveController = enemy.AddComponent<EnemyMoveController>();
            var controller = enemy.AddComponent<CharacterMoveController>();
            controller.Init(enemyIds[index]);
            //创建NPC控制器
            var enemyAIController = enemy.AddComponent<EnemyAIController>();

            enemyAIController.EmenyControllerInit(enemyNames[index], enemyIds[index], enemyTypes[index]);


            //启用敌人
            enemy.name = $"enemy{index}";
            enemy.tag = nameof(ObjectType.Enemy);
            enemy.SetActive(true);
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
                CompleteScene(true);
            }
        }
    }
}