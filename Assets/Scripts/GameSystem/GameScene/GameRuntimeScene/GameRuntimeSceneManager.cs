using System.Collections.Generic;
using Config;
using GameSystem.Character.common;
using GameSystem.Character.Enemy.EnemyAI;
using GameSystem.Character.Player;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameScene.GameEndScene;
using GameSystem.GameScene.MainMenu;
using GameSystem.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.GameScene.GameRuntimeScene
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

        /// <summary>游戏开始时间（用于结算时长）</summary>
        private float _gameStartTime;

        /// <summary>击杀计数：角色ID → 击杀数</summary>
        private Dictionary<string, int> _killCounts = new();

        /// <summary>死亡计数：角色ID → 死亡数</summary>
        private Dictionary<string, int> _deathCounts = new();

        /// <summary>角色等级：角色ID → 当前等级</summary>
        private Dictionary<string, int> _playerLevels = new();

        /// <summary>角色经验：角色ID → 当前经验值</summary>
        private Dictionary<string, int> _playerExps = new();

        /// <summary>角色存活状态：角色ID → 是否存活</summary>
        private Dictionary<string, bool> _aliveStatus = new();

        /// <summary>等待结算面板读取的结算数据（跨场景传递，静态字段不受场景卸载影响）</summary>
        public static GameResultData PendingGameResult { get; set; }


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
            // 注意：OnGameStateChanged 已在基类 BaseSceneManager.Start() 中订阅，此处不重复订阅
            GameEventSystem.AddListener<CharacterDieEvent>(OnGameCharacterDie);
            // 订阅等级/经验变化事件，用于计数板数据追踪
            GameEventSystem.AddListener<HUDEvent.LeaveUpEvent>(OnPlayerLevelUp);
            GameEventSystem.AddListener<HUDEvent.ExpAddEvent>(OnPlayerExpChanged);
        }

        private void OnDisable()
        {
            // 注意：OnGameStateChanged 已在基类 BaseSceneManager.OnDestroy() 中取消订阅
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnGameCharacterDie);
            GameEventSystem.RemoveListener<HUDEvent.LeaveUpEvent>(OnPlayerLevelUp);
            GameEventSystem.RemoveListener<HUDEvent.ExpAddEvent>(OnPlayerExpChanged);
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

            // 记录游戏开始时间
            _gameStartTime = Time.time;

            // 初始化所有计数板追踪数据
            _killCounts.Clear();
            _deathCounts.Clear();
            _playerLevels.Clear();
            _playerExps.Clear();
            _aliveStatus.Clear();
            // 清除上一次游戏的结算数据
            PendingGameResult = null;
            foreach (var info in CharacterBaseInfos)
            {
                _killCounts[info.CharacterId] = 0;
                _deathCounts[info.CharacterId] = 0;
                _playerLevels[info.CharacterId] = 1; // 初始等级为1
                _playerExps[info.CharacterId] = 0;
                _aliveStatus[info.CharacterId] = true;
            }

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
            PlayerController playerController;
            if (index == 0)
            {
                playerController = player.AddComponent<PlayerController>();
            }
            else
            {
                playerController = player.AddComponent<PlayerController>();
                playerController.DisableCamera();
            }

            playerController.PlayerControllerInit(CharacterBaseInfos[index].CharacterName, CharacterBaseInfos[index].CharacterId,
                CharacterBaseInfos[index].CharacterType, CharacterBaseInfos[index].CharacterControlConfig);
            
            //创建玩家移动控制器
            var controller = playerController.AddComponent<CharacterMoveController>();
            controller.Init(playerController.Id);
            //启用预制体
            player.name = $"player{index}";
            player.tag = nameof(ObjectType.Player);
            player.SetActive(true);
            player.transform.SetParent(transform);
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
            enemy.transform.SetParent(transform);
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
            // 记录击杀者（如果击杀者不是死亡者自身）
            if (!string.IsNullOrEmpty(evt.AttackerID) && evt.AttackerID != evt.DieId)
            {
                if (_killCounts.ContainsKey(evt.AttackerID))
                    _killCounts[evt.AttackerID]++;
                // 广播击杀者计数板更新
                BroadcastScoreBoardUpdate(evt.AttackerID);
            }

            // 记录死亡者死亡数
            if (_deathCounts.ContainsKey(evt.DieId))
                _deathCounts[evt.DieId]++;

            // 更新存活状态
            if (_aliveStatus.ContainsKey(evt.DieId))
                _aliveStatus[evt.DieId] = false;

            // 更新存活计数
            if (evt.DieId[0] == 'E')
                currentNPCCount--;
            else
                currentPlayerCount--;

            // 广播死亡者计数板更新
            BroadcastScoreBoardUpdate(evt.DieId);

            // 判断游戏结束条件
            if (currentPlayerCount == 0)
            {
                // 所有玩家阵亡 → 失败
                Debug.Log("[GameResult] 所有玩家阵亡，游戏结束 - 失败");
                BroadcastGameOver(false);
            }
            else if (currentNPCCount == 0 && currentPlayerCount > 0)
            {
                // 所有NPC阵亡且还有玩家存活 → 胜利
                Debug.Log("[GameResult] 所有敌人被消灭，游戏结束 - 胜利");
                BroadcastGameOver(true);
            }
        }

        /// <summary>
        ///     收集结算数据并广播 GameOverEvent
        /// </summary>
        private void BroadcastGameOver(bool isVictory)
        {
            // 构建结算数据
            var resultData = new GameResultData
            {
                IsPlayerVictory = isVictory,
                GameDuration = Time.time - _gameStartTime,
                MapName = GameModeSelect.Instance != null ? GameModeSelect.Map?.mapName ?? "未知" : "未知",
                GameMode = GameModeSelect.CurrentModeType.ToString()
            };

            foreach (var info in CharacterBaseInfos)
            {
                var cid = info.CharacterId;
                var playerResult = new PlayerResultData
                {
                    CharacterId = cid,
                    CharacterName = info.CharacterName,
                    CharacterType = info.CharacterType.ToString(),
                    IsAlive = _aliveStatus.ContainsKey(cid) && _aliveStatus[cid],
                    KillCount = _killCounts.ContainsKey(cid) ? _killCounts[cid] : 0,
                    DeathCount = _deathCounts.ContainsKey(cid) ? _deathCounts[cid] : 0,
                    FinalLevel = _playerLevels.ContainsKey(cid) ? _playerLevels[cid] : 1,
                    FinalExp = _playerExps.ContainsKey(cid) ? _playerExps[cid] : 0
                };
                resultData.PlayerResults.Add(playerResult);
            }

            // 将结算数据存入静态字段，供结算面板跨场景读取
            PendingGameResult = resultData;

            // 广播 GameOverEvent（结算面板监听此事件展示结果）
            GameEventSystem.Broadcast(new GameOverEvent
            {
                isWin = isVictory,
                ResultData = resultData
            });

            // 通知场景管理器标记场景完成 → 触发 GameFlowManager.EndGame
            CompleteScene(isVictory);
        }

        #region 计数板数据追踪

        /// <summary>
        ///     玩家升级时更新等级和经验追踪
        /// </summary>
        private void OnPlayerLevelUp(HUDEvent.LeaveUpEvent evt)
        {
            if (_playerLevels.ContainsKey(evt.Id))
                _playerLevels[evt.Id] = evt.Level;
            if (_playerExps.ContainsKey(evt.Id))
                _playerExps[evt.Id] = evt.EXP;
            BroadcastScoreBoardUpdate(evt.Id);
        }

        /// <summary>
        ///     玩家经验变化时更新经验追踪
        /// </summary>
        private void OnPlayerExpChanged(HUDEvent.ExpAddEvent evt)
        {
            if (_playerExps.ContainsKey(evt.Id))
                _playerExps[evt.Id] = evt.Exp;
            BroadcastScoreBoardUpdate(evt.Id);
        }

        /// <summary>
        ///     广播计数板更新事件（供实时计数板UI监听）
        /// </summary>
        private void BroadcastScoreBoardUpdate(string playerId)
        {
            var killCount = _killCounts.ContainsKey(playerId) ? _killCounts[playerId] : 0;
            var deathCount = _deathCounts.ContainsKey(playerId) ? _deathCounts[playerId] : 0;
            var level = _playerLevels.ContainsKey(playerId) ? _playerLevels[playerId] : 1;
            var exp = _playerExps.ContainsKey(playerId) ? _playerExps[playerId] : 0;
            var isAlive = _aliveStatus.ContainsKey(playerId) && _aliveStatus[playerId];

            GameEventSystem.Broadcast(new HUDEvent.ScoreBoardUpdateEvent(
                playerId, killCount, deathCount, level, exp, isAlive,
                currentPlayerCount, currentNPCCount));
        }

        #endregion

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