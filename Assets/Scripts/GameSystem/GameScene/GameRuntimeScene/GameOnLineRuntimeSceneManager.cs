using System;
using System.Collections.Generic;
using Config;
using Core.Net;
using GameSystem.GameScene.MainMenu.Character;
using GameSystem.GameScene.MainMenu.Character.Player;
using GameSystem.GameScene.MainMenu.EventSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameOnLineRuntimeSceneManagerManager : BaseSceneManager
    {
        [Header("初始组件")] [Tooltip("角色预制体")]
        public GameObject Character;

        [Tooltip("正交摄像机")]
        public Camera OrthographicCamera;
        
        [Tooltip("可破坏墙体")]
        public GameObject DestructibleWall;

        [Tooltip("玩家数量")] public int playerCount;

        [Tooltip("玩家控制配置")] public static List<CharacterBaseInfo> CharacterBaseInfos = new List<CharacterBaseInfo>();
        
        [Tooltip("其他玩家状态HUD")] public List<GameObject> huds;
        
        private int hudIndex = 0;
        
        [Tooltip("玩家状态HUD")] public GameObject hud;


        [Header("初始参数")]
        [Tooltip("当前玩家数")] public int currentPlayerCount;
        
        private List<GameObject> Characters = new List<GameObject>();
        public static GameOnLineRuntimeSceneManagerManager Instance { get; private set; }


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
                Debug.LogError("在GameOnLineRuntimeSceneManagerManager初始化过程中GameModeSelect为空");
            }
            else
            {
                playerCount = GameModeSelect.PlayerCount;
                if (playerCount > 4)
                {
                    Debug.LogError("在GameOnLineRuntimeSceneManagerManager初始化过程中玩家数量超过4");
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
            var idx = 0;
            foreach (var info in CharacterBaseInfos)
            {
                LoadPlayer(info, idx);
                idx++;
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
        }

        private void LoadPlayer(CharacterBaseInfo info, int idx)
        {
            //实例化游戏对象
            var player = Instantiate(Character, info.Spawn, Quaternion.Euler(0, info.Angle, 0));
            
            //创建玩家控制器
            PlayerController playerController;
            if (info.CharacterId == TcpGameClient.PlayerId)
            {
                playerController = player.AddComponent<OnlinePlayerController>();
                //获取HUD控制器
                hud.SetActive(true);
                var playerStateHUD = hud.GetComponent<PlayerStateHUD>();
                if (playerStateHUD == null) Debug.LogError("在GameOnLineRuntimeSceneManagerManager初始化过程中playerStateHUD为空");
                playerStateHUD.LoadHUD(info.CharacterId);
            }
            else
            {
                playerController = player.AddComponent<OnlineOtherPlayerController>();
                playerController.DisableCamera();
                (playerController as OnlineOtherPlayerController).PlayerId = info.CharacterId;
                //获取HUD控制器
                huds[hudIndex].SetActive(true);
                var playerStateHUD = huds[hudIndex].GetComponent<PlayerStateHUD>();
                if (playerStateHUD == null) Debug.LogError("在GameOnLineRuntimeSceneManagerManager初始化过程中playerStateHUD为空");
                playerStateHUD.LoadHUD(info.CharacterId);
                hudIndex++;
            }

            playerController.PlayerControllerInit(info.CharacterName, info.CharacterId,
                info.CharacterType, info.CharacterControlConfig);
            
            //创建玩家移动控制器
            var controller = playerController.AddComponent<CharacterMoveController>();
            controller.Init(playerController.id);
            //启用预制体
            player.name = $"player{idx}";
            player.tag = nameof(ObjectType.Player);
            player.SetActive(true);
            Characters.Add(player);
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
            currentPlayerCount--;

            if (currentPlayerCount > 1)
            {
            }
            else if (currentPlayerCount == 1)
            {
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
