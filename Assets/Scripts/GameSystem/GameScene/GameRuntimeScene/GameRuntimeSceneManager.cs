using System;
using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.Character;
using GameSystem.GameScene.MainMenu.Character.Enemy;
using GameSystem.GameScene.MainMenu.Character.Player;
using GameSystem.GameScene.MainMenu.EventSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameRuntimeSceneManager : BaseSceneManager
    {
        [Header("初始组件")] [Tooltip("角色预制体")]
        public GameObject Character;

        [Tooltip("正交摄像机")]
        public Camera OrthographicCamera;
        
        [Tooltip("可破坏墙体")]
        public GameObject DestructibleWall;

        [Tooltip("玩家数量")] public int playerCount;

        [Tooltip("NPC数量")] public int npcCount;
        
        [Tooltip("玩家控制配置")] public static List<CharacterBaseInfo> CharacterBaseInfos = new List<CharacterBaseInfo>();
        
        [Tooltip("玩家出生点")] public List<Transform> spawns;

        [Tooltip("玩家状态HUD")] public List<GameObject> huds;


        [Header("初始参数")]
        [Tooltip("当前玩家数")] public int currentPlayerCount;

        [Tooltip("当前NPC数")] public int currentNPCCount;

        
        private List<GameObject> Characters = new List<GameObject>();
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
            GameFlowManager.OnGameStateChanged += OnGameStateChanged;
            GameEventSystem.AddListener<CharacterDieEvent>(OnGameCharacterDie);
        }

        private void OnDisable()
        {
            GameFlowManager.OnGameStateChanged -= OnGameStateChanged;
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnGameCharacterDie);
        }
        

        public override void InitializeScene()
        {

            if (GameModeSelect.Instance == null)
            {
                Debug.LogError("在GameRuntimeSceneManager初始化过程中GameModeSelect为空");
            }
            else
            {
                playerCount = GameModeSelect.PlayerCount;
                npcCount = GameModeSelect.EnemyCount;
                if (npcCount + playerCount > 4)
                {
                    Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家数量/敌人数量超过4");
                }

                CharacterBaseInfos = GameModeSelect.CharacterBaseInfos;
                InitGame();
            }
        }
        
        public void InitGame()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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
                    LoadPlayer(0,0);
                    OrthographicCamera.gameObject.SetActive(false);
                    for (int i = 0; i < npcCount; i++)
                    {
                        LoadNPC(i + 1, i + 2);
                    }
                }
                else if(playerCount == 2)
                {
                    OrthographicCamera.gameObject.SetActive(true);
                    OrthographicCamera.targetDisplay = 0;
                    LoadPlayer(0,1);
                    LoadPlayer(1,3);
                    for (int i = 1; i <= npcCount; i++)
                    {
                        LoadNPC(1 + i, i + 2);
                    }
                }
                else if(playerCount == 3)
                {
                    OrthographicCamera.gameObject.SetActive(true);
                    OrthographicCamera.targetDisplay = 0;
                    LoadPlayer(0, 1);
                    LoadPlayer(1, 2);
                    LoadPlayer(2, 3);
                    if (npcCount == 1)
                    {
                        LoadNPC(3, 4);
                    }
                }
                else
                {
                    OrthographicCamera.gameObject.SetActive(true);
                    OrthographicCamera.targetDisplay = 0;
                    LoadPlayer(0, 1);
                    LoadPlayer(1, 2);
                    LoadPlayer(2, 3);
                    LoadPlayer(3, 4);
                }
            }
        }
        
        public override void CleanupScene()
        {
            foreach (var character in Characters)
            {
                if (character != null)
                {
                    Destroy(character);
                }
            }
            Characters.Clear();
            // 隐藏并锁定鼠标光标到屏幕中心
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // 注意：不要在CleanupScene中调用InitializeScene()
            // CleanupScene只在场景卸载时调用，不应该重新初始化场景
        }

        private void InitVariable()
        {
            if (spawns.Count == 5 || huds.Count == 5)
            {
                foreach (var vaSpawn in spawns)
                    if (vaSpawn == null)
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家出生点为空");

                foreach (var vaHud in huds)
                    if (vaHud == null)
                        Debug.LogError("在GameRuntimeSceneManager初始化过程中玩家状态HUD为空");
            }
            else
            {
                Debug.LogError("在GameRuntimeSceneManager初始化过程中出现出生点数量/HUD数量不足");
            }
        }

        private void LoadPlayer(int index,int oIndex)
        {
            //实例化游戏对象
            var player = Instantiate(Character, spawns[oIndex].position,
                spawns[oIndex].rotation);
            //获取HUD控制器
            huds[oIndex].SetActive(true);
            var playerStateHUD = huds[oIndex].GetComponent<PlayerStateHUD>();
            if (playerStateHUD == null) Debug.LogError("在GameRuntimeSceneManager初始化过程中playerStateHUD为空");
            playerStateHUD.LoadHUD(CharacterBaseInfos[index].CharacterId);
            
            //创建玩家控制器
            var playerController = player.AddComponent<PlayerController>();
            if (index != 0)
                playerController.DisableCamera();
            playerController.PlayerControllerInit(CharacterBaseInfos[index].CharacterName, CharacterBaseInfos[index].CharacterId,
                CharacterBaseInfos[index].CharacterType, CharacterBaseInfos[index].CharacterControlConfig);
            
            //创建玩家移动控制器
            var controller = playerController.AddComponent<CharacterMoveController>();
            controller.Init(playerController.id);
            //启用预制体
            player.name = $"player{index}";
            player.tag = nameof(ObjectType.Player);
            player.SetActive(true);
            Characters.Add(player);
        }

        private void LoadNPC(int index, int oIndex)
        {
            //实例化游戏对象
            var enemy = Instantiate(Instance.Character, spawns[oIndex].position,
                spawns[oIndex].rotation);
            //创建HUD
            var playerStateHUD = huds[oIndex].GetComponent<PlayerStateHUD>();
            if (playerStateHUD == null) Debug.LogError("在GameRuntimeSceneManager初始化过程中playerStateHUD为空");
            playerStateHUD.LoadHUD(CharacterBaseInfos[index].CharacterId);
            huds[index].SetActive(true);
            //初始化敌人移动控制器
            var moveController = enemy.AddComponent<EnemyMoveController>();
            var controller = enemy.AddComponent<CharacterMoveController>();
            controller.Init(CharacterBaseInfos[index].CharacterId);
            //创建NPC控制器
            var enemyAIController = enemy.AddComponent<EnemyAIController>();

            enemyAIController.EmenyControllerInit(CharacterBaseInfos[index].CharacterName, CharacterBaseInfos[index].CharacterId, CharacterBaseInfos[index].CharacterType);
            
            //启用敌人
            enemy.name = CharacterBaseInfos[index].CharacterName;
            enemy.tag = nameof(ObjectType.Enemy);
            enemy.SetActive(true);
            Characters.Add(enemy);
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
                /*
                CompleteScene(true);
            */
            }
            else if (currentPlayerCount == 0 && currentNPCCount != 0)
            {
                //NPC赢
                print("游戏结束");
                /*
                CompleteScene(true);
            */
            }
        }

        public override void PauseScene()
        {
            base.PauseScene();
            // 锁定鼠标
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

        }

        public override void ResumeScene()
        {
            base.ResumeScene();
            // 解除鼠标锁定
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}