using System.Collections.Generic;
using Config;
using Core.Net;
using GameSystem.Character.common;
using GameSystem.Character.Player;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameProps;
using GameSystem.GameProps.Item;
using GameSystem.GameScene.GameEndScene;
using GameSystem.GameScene.MainMenu;
using GameSystem.Manager;
using GameSystem.Map;
using GameSystem.Pool;
using GameSystem.Timer;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameOnLineRuntimeSceneManagerManager : BaseSceneManager
    {
        [Header("初始组件")] [Tooltip("角色预制体")]
        public GameObject Character;

        [Tooltip("正交摄像机")]
        public Camera OrthographicCamera;
        
        [Tooltip("玩家数量")] public int playerCount;

        [Tooltip("玩家控制配置")] public static List<CharacterBaseInfo> CharacterBaseInfos = new List<CharacterBaseInfo>();
        
        [Tooltip("其他玩家状态HUD")] public List<GameObject> huds;
        
        private int hudIndex = 0;
        
        [Tooltip("玩家状态HUD")] public GameObject hud;


        [Header("初始参数")]
        [Tooltip("当前玩家数")] public int currentPlayerCount;
        
        private List<GameObject> Characters = new List<GameObject>();
        public static GameOnLineRuntimeSceneManagerManager Instance { get; private set; }

        /// <summary>是否已完成初始化（防止重复初始化）</summary>
        private bool _isGameInitialized;

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

            // 注册在线道具系统消息处理器
            RegisterPropsHandlers();

            // 注册在线炸弹系统消息处理器
            RegisterBombHandlers();
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
            if (_isGameInitialized)
            {
                Debug.LogWarning("[GameOnLineRuntimeSceneManagerManager] InitGame 已执行过，跳过重复初始化");
                return;
            }
            _isGameInitialized = true;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            currentPlayerCount = playerCount;

            // 记录游戏开始时间
            _gameStartTime = Time.time;

            // 初始化所有计数板追踪数据
            _killCounts.Clear();
            _deathCounts.Clear();
            _playerLevels.Clear();
            _playerExps.Clear();
            _aliveStatus.Clear();
            // 清除上一次游戏的结算数据
            GameRuntimeSceneManager.PendingGameResult = null;
            foreach (var info in CharacterBaseInfos)
            {
                _killCounts[info.CharacterId] = 0;
                _deathCounts[info.CharacterId] = 0;
                _playerLevels[info.CharacterId] = 1;
                _playerExps[info.CharacterId] = 0;
                _aliveStatus[info.CharacterId] = true;
            }

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
            // 重置初始化标记，允许下次重新初始化
            _isGameInitialized = false;
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
                //获取HUD控制器
                huds[hudIndex].SetActive(true);
                var playerStateHUD = huds[hudIndex].GetComponent<PlayerStateHUD>();
                if (playerStateHUD == null) Debug.LogError("在GameOnLineRuntimeSceneManagerManager初始化过程中playerStateHUD为空");
                playerStateHUD.LoadHUD(info.CharacterId);
                hudIndex++;
            }
            (playerController as BaseOnlinePlayerController).PlayerId = info.CharacterId;

            playerController.PlayerControllerInit(info.CharacterName, info.CharacterId,
                info.CharacterType, info.CharacterControlConfig);

            if (info.CharacterId == TcpGameClient.PlayerId)
            {
                //创建玩家移动控制器
                var controller = playerController.AddComponent<CharacterMoveController>();
                controller.Init(playerController.Id);
            }
            
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
            // 记录击杀者
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
            currentPlayerCount--;

            // 广播死亡者计数板更新
            BroadcastScoreBoardUpdate(evt.DieId);

            // 在线模式：当只剩1名玩家存活时，该玩家胜利
            if (currentPlayerCount == 1)
            {
                Debug.Log("[GameResult-Online] 只剩最后一名玩家，游戏结束");
                BroadcastGameOver(true);
            }
        }

        /// <summary>
        ///     收集结算数据并广播 GameOverEvent
        /// </summary>
        private void BroadcastGameOver(bool isVictory)
        {
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
            GameRuntimeSceneManager.PendingGameResult = resultData;

            // 广播 GameOverEvent
            GameEventSystem.Broadcast(new GameOverEvent
            {
                isWin = isVictory,
                ResultData = resultData
            });

            // 通知场景完成
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
                currentPlayerCount, 0));
        }

        #endregion

        #region 在线道具系统

        /// <summary>
        ///     服务端生成的道具实例缓存（itemId → PropsStatus），用于被拾取时查找和移除
        /// </summary>
        private readonly Dictionary<string, PropsStatus> _spawnedProps = new Dictionary<string, PropsStatus>();

        /// <summary>
        ///     反向映射（PropsStatus → itemId），用于拾取时从 PropsStatus 实例反查服务端 itemId
        /// </summary>
        private readonly Dictionary<PropsStatus, string> _propToItemId = new Dictionary<PropsStatus, string>();

        /// <summary>
        ///     根据 PropsStatus 实例查找对应的服务端 itemId（供 BaseOnlinePlayerController 拾取时使用）
        /// </summary>
        public bool TryGetPropItemId(PropsStatus ps, out string itemId)
        {
            return _propToItemId.TryGetValue(ps, out itemId);
        }

        /// <summary>
        ///     注销道具（移除缓存和双向映射）
        /// </summary>
        public void UnregisterProp(string itemId)
        {
            if (_spawnedProps.TryGetValue(itemId, out var ps))
            {
                _propToItemId.Remove(ps);
                _spawnedProps.Remove(itemId);
            }
        }

        /// <summary>
        ///     注册在线道具系统的网络消息处理器
        /// </summary>
        private void RegisterPropsHandlers()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new(CmdType.PropSpawn, OnPropSpawn),
                new(CmdType.PropPickedUp, OnPropPickedUp),
                new(CmdType.PropEffectEnable, OnPropEffectEnable),
                new(CmdType.PropEffectDisable, OnPropEffectDisable)
            });
        }

        /// <summary>
        ///     处理服务端下发的道具生成消息
        /// </summary>
        private void OnPropSpawn(NetMessage msg)
        {
            var propsId = msg._body.GetString("propsId");
            var itemId = msg._body.GetString("itemId");
            var propsType = msg._body.GetString("propsType");
            var propsSize = msg._body.GetString("propsSize");
            var validTime = msg._body.GetFloat("validTime");
            var x = msg._body.GetInt("x") / 100f;
            var y = msg._body.GetInt("y") / 100f;
            var z = msg._body.GetInt("z") / 100f;
            var position = new Vector3(x, y, z);

            // 从本地资源加载对应的 PropsConfig
            if (!PropsManager.Instance.GetPropsConfigById(propsId, out var propsConfig))
            {
                Debug.LogWarning($"[PropsOnline] 找不到道具配置: {propsId}");
                return;
            }

            // 实例化道具 GameObject
            if (propsConfig.propsObj == null)
            {
                Debug.LogWarning($"[PropsOnline] 道具[{propsId}]缺少预制体引用");
                return;
            }

            var item = Instantiate(propsConfig.propsObj, position, Quaternion.identity);
            item.transform.SetParent(PropsManager.Instance.transform);

            var ps = item.GetComponent<PropsStatus>();
            if (ps == null)
            {
                Debug.LogError($"[PropsOnline] 道具预制体[{propsId}]上未找到 PropsStatus 组件");
                Destroy(item);
                return;
            }

            // 设置道具配置
            ps.propsConfig = propsConfig;
            ps.VirtualPosition = MapInfo.Instance.GetVirtualCoord(position);

            // 设置道具外观（材质颜色 + 大小，与离线模式 Destructible.CreateItem 保持一致）
            if (propsConfig.propsMaterial != null)
            {
                var renderers = ps.gameObject.GetComponentsInChildren<MeshRenderer>();
                for (var i = 0; i < renderers.Length; i++)
                {
                    var materials = renderers[i].materials;
                    for (var j = 0; j < materials.Length; j++)
                    {
                        materials[j].color = propsConfig.propsMaterial.color;
                    }
                }
            }

            switch (propsConfig.propsSize)
            {
                case PropsSize.Small:
                    ps.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case PropsSize.Medium:
                    ps.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    break;
                case PropsSize.Large:
                    ps.transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
                default:
                    Debug.LogWarning($"[PropsOnline] 未知道具大小: {propsConfig.propsSize}");
                    break;
            }

            // 注册到地图系统（可被拾取检测）
            MapInfo.Instance.AddItem(position, ps, TagType.Props);

            // 缓存以便后续查找
            _spawnedProps[itemId] = ps;
            _propToItemId[ps] = itemId;

            Debug.Log($"[PropsOnline] 服务端生成了道具[{propsId}]($itemId) 在({x}, {y}, {z})");
        }

        /// <summary>
        ///     处理服务端下发的道具被拾取消息
        /// </summary>
        private void OnPropPickedUp(NetMessage msg)
        {
            var itemId = msg._body.GetString("itemId");
            var playerId = msg._body.GetString("playerId");

            // 从缓存中查找（本地玩家拾取时可能已被 UnregisterProp 移除）
            if (!_spawnedProps.TryGetValue(itemId, out var ps))
            {
                // 本地玩家已提前清理，静默忽略
                return;
            }

            // 清理双向映射
            _propToItemId.Remove(ps);
            _spawnedProps.Remove(itemId);

            // 从地图系统和场景中移除（所有客户端都需要移除）
            MapInfo.Instance.RemoveItem(ps.transform.position, ps);
            Destroy(ps.gameObject);

            Debug.Log($"[PropsOnline] 道具[{itemId}]被玩家[{playerId}]拾取");
        }

        /// <summary>
        ///     处理服务端下发的道具效果启用消息
        ///     在线模式不使用临时 GameObject，直接通过 PropsConfig 广播事件
        ///     限时道具使用 GlobalTimerManager（时间戳模式）做本地自动过期保障
        /// </summary>
        private void OnPropEffectEnable(NetMessage msg)
        {
            var playerId = msg._body.GetString("playerId");
            var propsId = msg._body.GetString("propsId");

            Debug.Log($"[PropsOnline] 玩家[{playerId}]的道具[{propsId}]效果已启用");

            // 如果是本地玩家，广播 PropsStatusEnable 事件以触发 BaseState.ApplyPropsEffect
            if (playerId == TcpGameClient.PlayerId)
            {
                if (PropsManager.Instance.GetPropsConfigById(propsId, out var propsConfig))
                {
                    // 在线模式：直接广播事件（仅携带 PropsConfig，不创建临时 GameObject）
                    GameEventSystem.Broadcast(new PropsEvent.PropsStatusEnable(playerId, propsConfig));

                    // 限时道具（validTime > 0）：创建本地 Timer 作为自动过期保障
                    // 添加 3 秒缓冲确保服务端 PROP_EFFECT_DISABLE 先触发
                    if (propsConfig.validTime > 0f)
                    {
                        var timerKey = GetPropsTimerKey(playerId, propsConfig.propsId);
                        var expireTime = LocalTime.Now + (long)(propsConfig.validTime * 1000) + 3000L;
                        GlobalTimerManager.Instance.CreateOrResetTimer(timerKey, expireTime,
                            onComplete: () => OnOnlinePropsTimerExpired(playerId, propsConfig),
                            useTimeScale: false);
                    }
                }
            }
        }

        /// <summary>
        ///     处理服务端下发的道具效果禁用消息
        ///     取消本地 Timer（若存在），防止重复移除道具效果
        /// </summary>
        private void OnPropEffectDisable(NetMessage msg)
        {
            var playerId = msg._body.GetString("playerId");
            var propsId = msg._body.GetString("propsId");

            Debug.Log($"[PropsOnline] 玩家[{playerId}]的道具[{propsId}]效果已禁用");

            // 如果是本地玩家，广播 PropsStatusDisable 事件以触发 BaseState.ApplyPropsEffect 移除
            if (playerId == TcpGameClient.PlayerId)
            {
                if (PropsManager.Instance.GetPropsConfigById(propsId, out var propsConfig))
                {
                    var timerKey = GetPropsTimerKey(playerId, propsConfig.propsId);

                    // 尝试取消本地 Timer
                    //  成功 → 服务端先于 Timer 触发，广播 Disable
                    //  失败 → Timer 已自动过期（已广播过 Disable），跳过，防止重复移除
                    if (GlobalTimerManager.Instance.CancelTimer(timerKey))
                    {
                        GameEventSystem.Broadcast(new PropsEvent.PropsStatusDisable(playerId, propsConfig));
                    }
                }
            }
        }

        /// <summary>
        ///     在线模式道具 Timer 过期回调（本地 Timer 先于服务端触发时的兜底）
        /// </summary>
        private void OnOnlinePropsTimerExpired(string playerId, PropsConfig propsConfig)
        {
            Debug.Log($"[PropsOnline] 本地道具[{propsConfig.propsId}] Timer 到期，广播 Disable（兜底）");
            GameEventSystem.Broadcast(new PropsEvent.PropsStatusDisable(playerId, propsConfig));
        }

        /// <summary>
        ///     生成在线模式道具 Timer 唯一键，与离线 PropsStatus.GetTimerKey 格式一致
        /// </summary>
        private static string GetPropsTimerKey(string playerId, string propsId) => $"{playerId}_{propsId}";

        #endregion

        #region 在线炸弹系统

        /// <summary>
        ///     服务端炸弹ID → 客户端炸弹位置的映射（用于 BOMB_EXPLODE 匹配本地炸弹实例）
        /// </summary>
        private readonly Dictionary<string, Vector3> _serverBombPositions = new Dictionary<string, Vector3>();

        /// <summary>
        ///     注册在线炸弹系统的网络消息处理器
        /// </summary>
        private void RegisterBombHandlers()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new(CmdType.BombExplode, OnBombExplode)
            });
        }

        /// <summary>
        ///     注册服务端炸弹（PUT_BOMB 广播时记录服务端 bombId → 客户端位置，并取消本地计时器）
        /// </summary>
        public void RegisterServerBomb(string serverBombId, Vector3 position)
        {
            _serverBombPositions[serverBombId] = position;

            // 立即取消本地计时器，防止客户端 Bomb.Explode 与服务端 BOMB_EXPLODE 重复处理
            var bomb = FindBombAtPosition(position);
            if (bomb != null)
            {
                bomb.CancelInvoke("Explode");
                bomb.isOnlineBomb = true;
            }
        }

        /// <summary>
        ///     处理服务端下发的炸弹爆炸消息（BOMB_EXPLODE = 0x0502）
        ///     服务端已权威处理伤害和地图更新，客户端负责：
        ///     1. 根据服务端下发的障碍物列表直接清除可破坏方块
        ///     2. 播放爆炸视觉效果
        ///     3. 回收炸弹
        /// </summary>
        private void OnBombExplode(NetMessage msg)
        {
            var bombId = msg._body.GetString("bombId");
            var gridX = msg._body.GetInt("gridX");
            var gridZ = msg._body.GetInt("gridZ");
            var serverX = msg._body.GetInt("x");
            var serverZ = msg._body.GetInt("z");
            var obstaclesStr = msg._body.GetString("obstacles");

            // ===== 1. 根据服务端障碍物列表直接清除可破坏方块 =====
            if (!string.IsNullOrEmpty(obstaclesStr))
            {
                var obstaclePairs = obstaclesStr.Split(';');
                foreach (var pair in obstaclePairs)
                {
                    var parts = pair.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var ogx) && int.TryParse(parts[1], out var ogz))
                    {
                        // 将服务端网格坐标直接转换为客户端世界坐标
                        // 客户端 GetVirtualCoord: FloorToInt(worldX) + offsetDistance = gridX
                        // 逆推: worldX = gridX - offsetDistance + 0.5f
                        var worldX = ogx - MapInfo.Instance.offsetDistance + 0.5f;
                        var worldZ = ogz - MapInfo.Instance.offsetDistance + 0.5f;
                        var worldPos = new Vector3(worldX, 0f, worldZ);

                        var items = MapInfo.Instance?.GetMapDataTarget(worldPos);
                        if (items != null)
                        {
                            // 使用快照副本避免迭代时修改字典
                            var snapshot = new List<KeyValuePair<BaseObject, TagType>>(items);
                            foreach (var kv in snapshot)
                            {
                                if (kv.Value == TagType.Destructible)
                                {
                                    MapInfo.Instance?.RemoveItem(worldPos, kv.Key);
                                    var dest = kv.Key as Destructible;
                                    if (dest != null && DestructiblePool.Instance != null)
                                    {
                                        DestructiblePool.Instance.ReturnDestructible(dest);
                                    }
                                    Debug.Log($"[BombOnline] 服务端通知清除障碍物: grid=({ogx},{ogz}) world=({worldX},{worldZ})");
                                }
                            }
                        }
                    }
                }
            }

            // ===== 2. 查找本地炸弹实例并播放视觉效果 =====
            Bomb targetBomb = null;

            // 方式1：通过服务端 bombId 查找对应位置
            if (!string.IsNullOrEmpty(bombId) && _serverBombPositions.TryGetValue(bombId, out var cachedPos))
            {
                targetBomb = FindBombAtPosition(cachedPos);
                _serverBombPositions.Remove(bombId);
            }

            // 方式2：通过格子坐标匹配（备用）
            if (targetBomb == null)
            {
                // 服务端坐标 → 客户端坐标转换
                var clientX = (serverX / 100f);
                var clientZ = (serverZ / 100f);
                var clientPos = new Vector3(
                    Mathf.Ceil(clientX) - 0.5f,
                    0f,
                    Mathf.Ceil(clientZ) - 0.5f
                );
                targetBomb = FindBombAtPosition(clientPos);
            }

            if (targetBomb != null)
            {
                Debug.Log($"[BombOnline] 服务端炸弹[{bombId}]爆炸，客户端播放视觉效果");
                targetBomb.CleanupFromServer();
            }
            else
            {
                // 方式3：扫描所有炸弹实例（兜底方案，处理坐标/缓存不匹配的情况）
                var allBombs = FindObjectsOfType<Bomb>();
                foreach (var b in allBombs)
                {
                    if (!b.isExplode && b.isOnlineBomb && b.serverBombId == bombId)
                    {
                        targetBomb = b;
                        Debug.Log($"[BombOnline] 通过ID匹配找到炸弹[{bombId}]（兜底方案）");
                        break;
                    }
                }

                if (targetBomb != null)
                {
                    targetBomb.CleanupFromServer();
                }
                else
                {
                    Debug.LogWarning($"[BombOnline] 未找到服务端炸弹[{bombId}]的本地实例（可能已被本地计时器提前引爆）");
                }
            }
        }

        /// <summary>
        ///     在指定位置查找炸弹（MapInfo + BombManager 双重查找）
        /// </summary>
        private Bomb FindBombAtPosition(Vector3 position)
        {
            // 从 MapInfo 查找
            var items = MapInfo.Instance.GetMapDataTarget(position);
            if (items != null)
            {
                foreach (var kv in items)
                {
                    if (kv.Value == TagType.Bomb)
                    {
                        var bomb = kv.Key.GetComponent<Bomb>();
                        if (bomb != null && !bomb.isExplode)
                            return bomb;
                    }
                }
            }
            return null;
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
