using System;
using System.IO.Enumeration;
using Game_props; // 引入System命名空间，提供基础类和基类
using GameSystem;
using GameSystem.EventSystem;
using GameSystem.GameScene.GameRuntimeScene;
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
        [Tooltip("HUD模板（测试用)")] 
        public GameObject HUD;


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
        public float maxBombRecoveryTime = 2f;
        public float maxBombCooldown = 0.8f; //放置炸弹冷却时间

        
        [Header("角色状态")]
        public bool isDead = false;
        public bool isStaminaEmpty = false;

        [Header("控制器")]
        public PlayerMoveController moveController;
        public PlayerStateHUD playerStateHUD;
            
        #endregion
        
        # region 事件监听设置
        private void OnEnable()
        {
            GameEventSystem.AddListener<PlayerTakeDamageEvent>(OnTakeDamage);
            GameEventSystem.AddListener<PlayerDieEvent>(OnPlayerDie);
            GameEventSystem.AddListener<PlayerDieEvent>(OnKillPlayer);
            GameEventSystem.AddListener<ExpAddEvent>(OnExpAdd);
            GameEventSystem.AddListener<LeaveUpEvent>(OnLeaveUp);
        }

        private void OnDisable()
        {
            GameEventSystem.RemoveListener<PlayerTakeDamageEvent>(OnTakeDamage);
            GameEventSystem.RemoveListener<PlayerDieEvent>(OnPlayerDie);
            GameEventSystem.RemoveListener<PlayerDieEvent>(OnKillPlayer);
            GameEventSystem.RemoveListener<ExpAddEvent>(OnExpAdd);
            GameEventSystem.RemoveListener<LeaveUpEvent>(OnLeaveUp);
        }
        #endregion

        #region 初始化函数
        private void Awake()
        {
            
            moveController = GetComponent<PlayerMoveController>();
            if (HUD == null)
            {
                Debug.LogError("HUD为空");
            }
            else
            {
                playerStateHUD = HUD.GetComponent<PlayerStateHUD>();
            }
            if (playerStateHUD == null)
            {
                Debug.LogError("playerStateHUD为空");
            }
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
            print("玩家初始化成功");
            InitHUD();
            print("HUD初始化成功");
        }  


        private void InitHUD()
        {
            playerStateHUD.PlayerController = this;
            playerStateHUD.ownerId = playerId;
            playerStateHUD.LoadHUD();
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
            bombDamage = playerProper.bombDamage;
            bombRadius = playerProper.bombRadius;
            bombFuseTime = playerProper.bombFuseTime;
            bombCooldown = playerProper.bombCooldown;
            bombRecoveryTime = playerProper.bombRecoveryTime;
            print("成功加载炸弹属性");
    
            // ========== 最大值设置 ==========
            maxHp = playerProper.maxHp;
            maxStamina = playerProper.maxStamina;
            maxBombCount = playerProper.maxBombCount;
            maxBombCooldown = playerProper.bombCooldown;
            maxBombRecoveryTime = playerProper.bombRecoveryTime;
            print("成功加载成长值");
    
            // ========== 自动恢复属性设置 ==========

            staminaDrainRate = playerProper.staminaDrainRate;
            staminaRegenRate = playerProper.staminaRegenRate;
            speedMultiplier = playerProper.speedMultiplier;
            print("成功加载自动恢复属性");
        }


        #endregion
        
        
        private void Update()
        { 
            if(isDead) return;//如果玩家死亡，则不执行以下代码
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PutBomb();
            }
            StaminaUpdate();
            BombUpdate();
        }

        private void StaminaUpdate()
        {
            if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !isStaminaEmpty){
                currentSpeed = baseSpeed * speedMultiplier;
                stamina = Mathf.Max(0f, stamina - staminaDrainRate * Time.deltaTime); //体力减少
                if (stamina == 0)
                {
                    isStaminaEmpty = true;
                    print("体力耗尽，停止冲刺"); 
                }
            }
            else
            {
                currentSpeed = baseSpeed;
                stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.deltaTime);
                moveController.MoveSpeed = baseSpeed;
                if (isStaminaEmpty && stamina > 20)
                {
                    isStaminaEmpty = false;
                    print("体力恢复，开始冲刺"); 
                }
                
            }
            moveController.MoveSpeed = currentSpeed;
            playerStateHUD.UpdateStamina(stamina, maxStamina, currentSpeed);
        }

        //采用倒数计数
        private void BombUpdate()
        {
            //炸弹放置冷却
            if (bombCooldown > 0)
            {
                bombCooldown = Mathf.Max(0f, bombCooldown - Time.deltaTime);
            }
            
            
            //炸弹恢复时间
            if (bombRecoveryTime > 0 && bombCount != maxBombCount)
            {
                bombRecoveryTime = Mathf.Max(0f, bombRecoveryTime - Time.deltaTime);
            }
            
            //炸弹数量恢复
            if (bombCount < maxBombCount && bombRecoveryTime == 0)
            {
               bombCount++;
               bombRecoveryTime = maxBombRecoveryTime; 
            }
            playerStateHUD.UpdateBomb(bombCooldown, bombCount,maxBombCount , bombRecoveryTime);
        }
        
        
        private void PutBomb()
        {
            if (bombCooldown > 0 || bombCount == 0)
            {
                print("炸弹冷却或数量为0，放置失败");
                return;
            }

            bombCooldown = maxBombCooldown;
            bombCount--;
            Vector3 bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0.5f;
            Collider[] hitColliders = Physics.OverlapBox(bombPos, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity);
            if (hitColliders.Length > 0)
            {
                foreach (var collider1 in hitColliders)
                {
                    if (!collider1.gameObject.CompareTag("Player"))
                    {
                        print("炸弹放置失败，位置有障碍物");
                        return;
                    }
                        
                }
            }
            bombPos.y = 0f;
            print("炸弹放置位置:" + bombPos);
            GameEventSystem.Broadcast(new BombPlaceRequestEvent 
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
                bombFuseTime = Mathf.Max(0.1f , bombFuseTime - playerProper.bombFuseTimeGrowth); // 修改：减少爆炸时间
                bombRadius += playerProper.bombRadiusGrowth; // 增加爆炸范围
                bombDamage += playerProper.bombDamageGrowth; // 增加炸弹伤害
                //更新炸弹冷却时间
                maxBombCooldown = Mathf.Max(0.1f, maxBombCooldown - playerProper.bombCooldownGrowth);
        
                //更新炸弹恢复时间
                maxBombRecoveryTime = Mathf.Max(0.5f, maxBombRecoveryTime - playerProper.bombRecoveryTimeGrowth);

                //最大值更新
                maxBombCount += playerProper.maxBombCountGrowth; // 炸弹数量
                maxStamina += playerProper.staminaGrowth; // 更新最大体力
                maxHp += playerProper.maxHpGrowth; // 最大生命值
                //更新UI
                playerStateHUD.OnLeaveUpUIUpdate(hp,maxHp,stamina,maxStamina,exp,level,globalProper.maxExpToLevelUp,currentSpeed,bombCount,maxBombCount,bombRecoveryTime,bombDamage,bombRadius,bombFuseTime);
            }
            else
            {
                //TODO 其他玩家升级时界面更新
            }
        }

        private void OnExpAdd(ExpAddEvent evt) //经验值增加事件
        {
            
            if (evt.PlayerId == playerId)
            {
                exp += evt.Exp;
                print( $"{evt.PlayerId} 玩家经验值增加 {evt.Exp} ,当前经验值 {exp}");
                
                if (level < playerProper.maxLevel && exp >= globalProper.maxExpToLevelUp)
                {
                    GameEventSystem.Broadcast(new LeaveUpEvent() { PlayerId = evt.PlayerId });
                }else if (level >= playerProper.maxLevel)
                {
                    print($"玩家{playerId}等级已满，无法升级");
                    exp = Mathf.Min(exp, globalProper.maxExpToLevelUp);
                    playerStateHUD.OnExpAddUIUpdate(exp, globalProper.maxExpToLevelUp);
                }
                else
                {
                    playerStateHUD.OnExpAddUIUpdate(exp, globalProper.maxExpToLevelUp);
                }
                
            }
        }
        
        
        private void OnKillPlayer(PlayerDieEvent evt) // 玩家死亡事件 击杀
        {
            if (evt.DieId != playerId && evt.AttackerID == playerId)
            {
                GameEventSystem.Broadcast(new ExpAddEvent()
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
                isDead = true;
                Die();
                playerId = -10;
                Destroy(moveController);
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
                    playerStateHUD.OnTakeDamageUIUpdate(hp ,maxHp);
                    print( playerId+"玩家死亡");
                    GameEventSystem.Broadcast(new PlayerDieEvent()
                    {
                        AttackerID = evt.OwnerId,
                        DieId = playerId,
                        Exp = 50 * level
                    });
                }
                else
                    playerStateHUD.OnTakeDamageUIUpdate(hp ,maxHp);
            }
        }

        private void Die()
        {
            GameEventSystem.RemoveListener<PlayerTakeDamageEvent>(OnTakeDamage);
            GameEventSystem.RemoveListener<PlayerDieEvent>(OnPlayerDie);
            GameEventSystem.RemoveListener<PlayerDieEvent>(OnKillPlayer);
            GameEventSystem.RemoveListener<ExpAddEvent>(OnExpAdd);
            GameEventSystem.RemoveListener<LeaveUpEvent>(OnLeaveUp);
        }
        
        #endregion
    }
}