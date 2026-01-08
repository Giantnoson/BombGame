using System;
using Game_props; // 引入System命名空间，提供基础类和基类
using GameSystem;
using TMPro;
// 引入GameSystem命名空间，可能包含游戏相关的系统类
using UnityEngine;                        // 引入Unity引擎的核心命名空间
using UnityEngine.PlayerLoop;            // 引入Unity的玩家循环命名空间，用于控制游戏逻辑更新顺序
using UnityEngine.Serialization;
using UnityEngine.UI; // 引入Unity的序列化命名空间，用于对象序列化和反序列化

namespace player
{
    public class PlayerController : MonoBehaviour
    {
        
        # region 变量定义
        [Header("基础模型")]
        [Tooltip("角色类型")]
        public PlayType playerType;
        [Tooltip("角色基础属性配置")]
        public PlayerProper playerProper;
        [Header("全局属性")]
        public GlobalProper globalProper;
        [Tooltip("玩家ID")]
        public int playerId;


        [Header("属性设置")]
        [Tooltip("生命值")]
        public float hp;
        [Tooltip("体力")]
        public float stamina = 100f;
        [Tooltip("炸弹数量")]
        public int bombCount = 0;
        [Tooltip("等级")]
        public int level = 1;
        [Tooltip("经验值")]
        public int exp = 0;
        [Tooltip("基础速度")]
        public float baseSpeed;
        [Tooltip("当前速度")]
        public float currentSpeed;
        
        [Header("炸弹设置")]
        [Tooltip("炸弹伤害")]
        public float bombDamage = 20f; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public int bombRadius = 5; //爆炸范围
        [Tooltip("爆炸时间")]
        public float bombFuseTime = 3f;//爆炸时间
        
        
        [Header("自动恢复属性设置")]
        [Tooltip("放置炸弹冷却时间")]
        public float bombCooldown = 0.8f; //放置炸弹冷却时间
        [Tooltip("炸弹恢复时间")]
        public float bombRecoveryTime = 2f;
        [Tooltip("体力消耗速率")]
        public float staminaDrainRate = 10f; //体力消耗速率
        [Tooltip("体力恢复速率")]
        public float staminaRegenRate = 10f; //体力恢复速率
        [Tooltip("速度倍率")]
        public float speedMultiplier = 1.2f; //速度倍率
        
        
        [Header("最大值设置")]
        [Tooltip("最大生命值")]
        public float maxHp = 100; //最大生命值
        [Tooltip("体力上限")]
        public float maxStamina = 100f; //体力上限
        [Tooltip("最大炸弹数")]
        public int maxBombCount = 5; //最大炸弹数量
        

        [Header("控制器")]
        public PlayerMoveController moveController;


        [Header("UI显示")]
        public TextMeshProUGUI hpText;
        public Image hpBar;
        public TextMeshProUGUI staminaText;
        public Image staminaBar;
        public TextMeshProUGUI expText;
        public Image expBar;
        public TextMeshProUGUI bombCountText;
        public TextMeshProUGUI bombDamageText;
        public TextMeshProUGUI bombRadiusText;
        public TextMeshProUGUI leaveText;
        public TextMeshProUGUI moveSpeedText;
        public TextMeshProUGUI cooldownText;
        public TextMeshProUGUI bombFuseTimeText;
            
        #endregion
        
        # region 事件监听设置
        private void OnEnable()
        {
            EventSystem.AddListener<PlayerTakeDamageEvent>(OnTakeDamage);
            EventSystem.AddListener<PlayerDieEvent>(OnPlayerDie);
            EventSystem.AddListener<PlayerDieEvent>(OnKillPlayer);
            EventSystem.AddListener<ExpAddEvent>(OnExpAdd);
            EventSystem.AddListener<LeaveUpEvent>(OnLeaveUp);
        }

        private void OnDisable()
        {
            EventSystem.RemoveListener<PlayerTakeDamageEvent>(OnTakeDamage);
            EventSystem.RemoveListener<PlayerDieEvent>(OnPlayerDie);
            EventSystem.RemoveListener<PlayerDieEvent>(OnKillPlayer);
            EventSystem.RemoveListener<ExpAddEvent>(OnExpAdd);
            EventSystem.RemoveListener<LeaveUpEvent>(OnLeaveUp);
        }
        #endregion

        #region 初始化函数
        private void Awake()
        {
            moveController = GetComponent<PlayerMoveController>();
            if (moveController == null)
            {
                Debug.LogError("moveController为空");
            }
        }

        private void Start()
        {

            //严格按照此顺序初始化
            InitProper();
            LoadProper();
            InitUI();
            print("玩家初始化成功");
        }
        
        private void LoadProper()
        {
            // ========== 初始值设定 ==========
            // 基础属性初始化
            hp = playerProper.maxHp; // 初始化生命值
            baseSpeed = playerProper.speed; // 初始化速度
            currentSpeed = baseSpeed; // 初始化当前速度
            bombCount = globalProper.initBombCount; // 初始化炸弹数量
            level = globalProper.initLevel; // 初始化等级
            exp = globalProper.initExp; // 初始化经验值
            stamina = playerProper.maxStamina; // 初始化体力值
    
            // ========== 数值传递 ==========
            // 移动控制器设置
            moveController.MoveSpeed = baseSpeed;
            print("成功加载玩家属性");
    
            // 注意：不再直接修改预制体上的Bomb组件属性
            // 属性将在创建炸弹实例时通过BombPlaceRequestEvent传递
            bombDamage = playerProper.bombDamage;
            bombRadius = playerProper.bombRadius;
            bombFuseTime = playerProper.bombFuseTime;
            print("成功加载炸弹属性");
    
            // ========== 最大值设置 ==========
            maxHp = playerProper.maxHp;
            maxStamina = playerProper.maxStamina;
            maxBombCount = playerProper.maxBombCount;
            print("成功加载成长值");
    
            // ========== 自动恢复属性设置 ==========
            bombCooldown = playerProper.bombCooldown;
            bombRecoveryTime = playerProper.bombRecoveryTime;
            staminaDrainRate = playerProper.staminaDrainRate;
            staminaRegenRate = playerProper.staminaRegenRate;
            speedMultiplier = playerProper.speedMultiplier;
            print("成功加载自动恢复属性");
        }

        private void InitProper()
        {
            switch (playerType) {
                case PlayType.Balance:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.Balance));
                    break;
                case PlayType.Speed:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.Speed));
                    break;
                case PlayType.BombTruck:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.BombTruck));
                    break;
                case PlayType.Tank:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.Tank));
                    break;
                default:
                    Debug.LogError("未知的角色类型");
                    break;
            }
            if (playerProper == null)
                Debug.LogError("在Resources中无法找到对应的PlayerProper");
            print("成功加载PlayerProper");
            globalProper = Resources.Load<GlobalProper>("GlobalProper");
            if (globalProper == null)
                Debug.LogError("在Resources中无法找到GlobalProper");
        }

        private void InitUI()
        {
            // 验证UI引用
            ValidateReference(hpText, "hpText");
            ValidateReference(hpBar, "hpBar");
            ValidateReference(staminaText, "staminaText");
            ValidateReference(staminaBar, "staminaBar");
            ValidateReference(expText, "expText");
            ValidateReference(expBar, "expBar");
            ValidateReference(bombCountText, "bombCountText");
            ValidateReference(bombDamageText, "bombDamageText");
            ValidateReference(bombRadiusText, "bombRadiusText");
            ValidateReference(leaveText, "leaveText");
            ValidateReference(moveSpeedText, "moveSpeedText");
            ValidateReference(cooldownText, "cooldownText");
            ValidateReference(bombFuseTimeText, "bombFuseTimeText");
            // 初始化HP相关
            hpText.text = $"{hp}/{playerProper.maxHp}";
            hpBar.fillAmount = hp / playerProper.maxHp;
    
            // 初始化体力相关
            staminaText.text = $"{stamina}/{playerProper.maxStamina}";
            staminaBar.fillAmount = stamina / playerProper.maxStamina;
    
            // 初始化经验值相关
            expText.text = $"{exp}/{globalProper.maxExpToLevelUp}";
            expBar.fillAmount = (float)exp / globalProper.maxExpToLevelUp;
    
            // 初始化炸弹相关
            bombCountText.text = bombCount.ToString();
            bombDamageText.text = playerProper.bombDamage.ToString();
            bombRadiusText.text = playerProper.bombRadius.ToString();
            bombFuseTimeText.text = playerProper.bombFuseTime.ToString("F2");
    
            // 初始化其他属性
            leaveText.text = level.ToString();
            moveSpeedText.text = baseSpeed.ToString();
            cooldownText.text = playerProper.bombCooldown.ToString("F2");
        }
        
        private void ValidateReference(object obj, string name)
        {
            if (obj == null) Debug.LogError($"{name}为空");
        }
        
        #endregion
        


        private void Update()
        { 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PutBomb();
            }
        }
        private void PutBomb()
        {
            Vector3 bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0.5f;
            Collider[] hitColliders = Physics.OverlapBox(bombPos, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity);
            if (hitColliders.Length > 0)
            {
                foreach (var variablCollider in hitColliders)
                {
                    if (!variablCollider.gameObject.CompareTag("Player"))
                    {
                        print("炸弹放置失败，位置有障碍物");
                        return;
                    }
                        
                }
            }
            bombPos.y = 0f;
            print("炸弹放置位置:" + bombPos);
            EventSystem.Broadcast(new BombPlaceRequestEvent 
            { 
                Position = bombPos, 
                OwnerId = playerId,
                BombFuseTime = bombFuseTime,
                BombRadius = bombRadius,
                BombDamage = bombDamage 
            });
        }

        
        
        #region 事件监听
        private void OnLeaveUp(LeaveUpEvent evt)
        {
            if (evt.PlayerId == playerId)
            {
                print("玩家升级，当前等级:" + level);
                exp -= globalProper.maxExpToLevelUp;
                level++;
        
                //基础数值更新
                hp += playerProper.maxHpGrowth; // 生命值
                baseSpeed += playerProper.speedGrowth; // 速度
                moveController.MoveSpeed = baseSpeed; //更新速度
                
                //炸弹数值更新
                bombFuseTime = Mathf.Max(0.1f , playerProper.bombFuseTime - playerProper.bombFuseTimeGrowth * level); // 修改：减少爆炸时间
                bombRadius += playerProper.bombRadiusGrowth; // 增加爆炸范围
                bombDamage += playerProper.bombDamageGrowth; // 增加炸弹伤害
                //更新炸弹冷却时间
                bombCooldown = Mathf.Max(0.1f, playerProper.bombCooldown - playerProper.bombCooldownGrowth * level);
        
                //更新炸弹恢复时间
                bombRecoveryTime = Mathf.Max(0.5f, playerProper.bombRecoveryTime - playerProper.bombRecoveryTimeGrowth * level);

                //最大值更新
                maxBombCount += playerProper.maxBombCountGrowth; // 炸弹数量
                maxStamina += playerProper.staminaGrowth; // 更新最大体力
                maxHp += playerProper.maxHpGrowth; // 最大生命值
                //更新UI
                OnLeaveUpUIUpdate();
            }
            else
            {
                //TODO 其他玩家升级时界面更新
            }
        }

        private void OnLeaveUpUIUpdate()
        {
            // ========== 生命值更新 ==========
            hpText.text = $"{hp}/{maxHp}";
            hpBar.fillAmount = hp / maxHp;
    
            // ========== 体力值更新 ==========
            staminaText.text = $"{stamina}/{maxStamina}";
            staminaBar.fillAmount = stamina / maxStamina;
    
            // ========== 经验值更新 ==========
            expText.text = $"{exp}/{globalProper.maxExpToLevelUp}";
            expBar.fillAmount = (float)exp / globalProper.maxExpToLevelUp;
    
            // ========== 等级更新 ==========
            leaveText.text = level.ToString();
    
            // ========== 速度更新 ==========
            moveSpeedText.text = baseSpeed.ToString();
    
            // ========== 炸弹属性更新 ==========
            bombCountText.text = bombCount.ToString();
            bombDamageText.text = bombDamage.ToString();
            bombRadiusText.text = bombRadius.ToString();
            bombFuseTimeText.text = bombFuseTime.ToString("F2"); // 保留两位小数
            cooldownText.text = bombCooldown.ToString("F2"); // 保留两位小数
        }

        private void OnExpAdd(ExpAddEvent evt) //经验值增加事件
        {
            
            if (evt.PlayerId == playerId)
            {
                exp += evt.Exp;
                print( $"{evt.PlayerId} 玩家经验值增加 {evt.Exp} ,当前经验值 {exp}");
                
                if (level < playerProper.maxLevel && exp >= globalProper.maxExpToLevelUp)
                {
                    EventSystem.Broadcast(new LeaveUpEvent() { PlayerId = evt.PlayerId });
                }else if (level >= playerProper.maxLevel)
                {
                    print($"玩家{playerId}等级已满，无法升级");
                    exp = Mathf.Min(exp, globalProper.maxExpToLevelUp);
                    OnExpAddUIUpdate();
                }
                else
                {
                    OnExpAddUIUpdate();
                }
                
            }
        }

        private void OnExpAddUIUpdate()
        {
            expText.text = $"{exp}/{globalProper.maxExpToLevelUp}";
            expBar.fillAmount = (float)exp / globalProper.maxExpToLevelUp;
        }
        
        private void OnKillPlayer(PlayerDieEvent evt) // 玩家死亡事件 击杀
        {
            if (evt.DieId != playerId && evt.AttackerID == playerId)
            {
                EventSystem.Broadcast(new ExpAddEvent()
                {
                    PlayerId = evt.AttackerID,
                    Exp = evt.Exp
                });
            }
        }
        
        private void OnPlayerDie(PlayerDieEvent evt)//玩家死亡事件 死亡
        {
            if (evt.DieId == playerId)
            {
                Destroy(gameObject);//销毁玩家
            }
        }

        private void OnTakeDamage(PlayerTakeDamageEvent evt) // 玩家受伤事件
        {
            if (evt.HitId == playerId)
            {

                hp -= evt.Damage;
                print($"{evt.HitId} 玩家受到来自 {evt.OwnerId}的 {evt.Damage} 伤害。剩余血量为: {hp}");//性能貌似没有之前print(evt.HitId+" 玩家受到来自 " + evt.OwnerId +" 伤害" + evt.Damage + " 剩余血量为: " + hp);高
                if (hp <= 0)//当玩家死亡时
                {
                    hp = 0;
                    OnTakeDamageUIUpdate();
                    print( playerId+"玩家死亡");
                    EventSystem.Broadcast(new PlayerDieEvent()
                    {
                        AttackerID = evt.OwnerId,
                        DieId = playerId,
                        Exp = 50 * level
                    });
                }
                else
                    OnTakeDamageUIUpdate();
            }
        }

        private void OnTakeDamageUIUpdate()
        {
            hpText.text = $"{hp}/{maxHp}";
            hpBar.fillAmount = hp / maxHp;
        }
        #endregion
        
    }
}