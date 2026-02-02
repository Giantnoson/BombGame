using Config;
using GameSystem.EventSystem;
using UnityEngine;

namespace GameSystem.Character
{
    public abstract class BaseState : BaseObject, IState
    {
        [Header("判断")] [Tooltip("炸弹更新结束")] public bool isBombUpdate;

        [Tooltip("移动更新结束")] private float beforeCurrentSpeed;

        private float beforeStamina;


        /// <summary>
        ///     初始化状态
        /// </summary>
        public virtual void StateInit()
        {
            InitProper();
            LoadProper();
            InitHUD();
            print("HUD初始化成功");
        }

        private void InitHUD()
        {
            GameEventSystem.Broadcast(new HUDEvent.LoadHUDEvent(id, characterName, characterType, characterProper,
                globalProper, hp, stamina, exp, level, currentSpeed));
        }

        private void InitProper()
        {
            switch (characterType)
            {
                case CharacterType.Balance:
                    characterProper = Resources.Load<CharacterProper>("Character/" + nameof(CharacterType.Balance));
                    break;
                case CharacterType.Speed:
                    characterProper = Resources.Load<CharacterProper>("Character/" + nameof(CharacterType.Speed));
                    break;
                case CharacterType.BombTruck:
                    characterProper = Resources.Load<CharacterProper>("Character/" + nameof(CharacterType.BombTruck));
                    break;
                case CharacterType.Tank:
                    characterProper = Resources.Load<CharacterProper>("Character/" + nameof(CharacterType.Tank));
                    break;
                case CharacterType.Enemy:
                    characterProper = Resources.Load<CharacterProper>("Character/" + nameof(CharacterType.Enemy));
                    break;
                default:
                    Debug.LogError("未知的角色类型");
                    break;
            }

            if (characterProper == null)
                Debug.LogError("在Resources中无法找到对应的CharacterProper");
            Debug.Log("成功加载CharacterProper");
            globalProper = Resources.Load<GlobalProper>("Character/" + "GlobalProper");
            if (globalProper == null)
                Debug.LogError("在Resources中无法找到GlobalProper");
        }

        private void LoadProper()
        {
            // 基础属性初始化
            hp = characterProper.maxHp; // 初始化生命值
            baseSpeed = characterProper.speed; // 初始化速度
            currentSpeed = baseSpeed; // 初始化当前速度
            bombCount = globalProper.initBombCount; // 初始化炸弹数量
            level = globalProper.initLevel; // 初始化等级
            exp = globalProper.initExp; // 初始化经验值
            stamina = characterProper.maxStamina; // 初始化体力值
            Debug.Log("成功加载玩家属性");
            bombDamage = characterProper.bombDamage;
            bombRadius = characterProper.bombRadius;
            bombFuseTime = characterProper.bombFuseTime;
            bombCooldown = characterProper.bombCooldown;
            bombRecoveryTime = characterProper.bombRecoveryTime;
            Debug.Log("成功加载炸弹属性");

            maxHp = characterProper.maxHp;
            maxStamina = characterProper.maxStamina;
            maxBombCount = characterProper.maxBombCount;
            maxBombCooldown = characterProper.bombCooldown;
            maxBombRecoveryTime = characterProper.bombRecoveryTime;
            Debug.Log("成功加载成长值");

            staminaDrainRate = characterProper.staminaDrainRate;
            staminaRegenRate = characterProper.staminaRegenRate;
            speedMultiplier = characterProper.speedMultiplier;
            Debug.Log("成功加载自动恢复属性");

            GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateSpeedEvent(id, currentSpeed));
        }

        protected void StaminaUpdate()
        {
            if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !isStaminaEmpty)
            {
                currentSpeed = baseSpeed * speedMultiplier;
                stamina = Mathf.Max(0f, stamina - staminaDrainRate * Time.deltaTime); //体力减少
                if (currentSpeed != beforeCurrentSpeed)
                {
                    GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateSpeedEvent(id, currentSpeed));
                    beforeCurrentSpeed = currentSpeed;
                }

                if (stamina != beforeStamina)
                {
                    GameEventSystem.Broadcast(new HUDEvent.UpdateStaminaEvent(id, stamina, maxStamina, currentSpeed));
                    beforeStamina = stamina;
                }

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
                if (isStaminaEmpty && stamina > 20)
                {
                    isStaminaEmpty = false;
                    print("体力恢复，开始冲刺");
                }

                if (currentSpeed != beforeCurrentSpeed)
                {
                    GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateSpeedEvent(id, currentSpeed));
                    beforeCurrentSpeed = currentSpeed;
                }

                if (stamina != beforeStamina)
                {
                    GameEventSystem.Broadcast(new HUDEvent.UpdateStaminaEvent(id, stamina, maxStamina, currentSpeed));
                    beforeStamina = stamina;
                }
            }
        }

        //采用倒数计数
        protected void BombUpdate()
        {
            //炸弹放置冷却
            if (bombCooldown > 0) bombCooldown = Mathf.Max(0f, bombCooldown - Time.deltaTime);
            //炸弹恢复时间
            if (bombRecoveryTime > 0 && bombCount != maxBombCount)
                bombRecoveryTime = Mathf.Max(0f, bombRecoveryTime - Time.deltaTime);

            //炸弹数量恢复
            if (bombCount < maxBombCount && bombRecoveryTime == 0)
            {
                bombCount++;
                bombRecoveryTime = maxBombRecoveryTime;
            }

            if (isBombUpdate) return;
            if (!isBombUpdate && bombCooldown == 0 && bombRecoveryTime == maxBombRecoveryTime &&
                bombCount == maxBombCount) isBombUpdate = true;
            GameEventSystem.Broadcast(new HUDEvent.UpdateBombEvent(id, bombCooldown, bombCount, maxBombCount,
                bombRecoveryTime));
        }

        #region 模板相关

        [Tooltip("角色类型")] public CharacterType characterType;

        [Tooltip("角色基础属性配置")] public CharacterProper characterProper;

        /// <summary>
        ///     全局属性
        /// </summary>
        [Tooltip("全局属性")] public GlobalProper globalProper;

        [Tooltip("玩家名称")] public string characterName;

        #endregion

        #region 基础属性相关

        /// <summary>
        ///     生命值
        /// </summary>
        [Header("属性设置")] [Tooltip("生命值")] public float hp;

        /// <summary>
        ///     等级
        /// </summary>
        [Tooltip("等级")] public int level = 1;

        /// <summary>
        ///     经验值
        /// </summary>
        [Tooltip("经验值")] public int exp;

        /// <summary>
        ///     最大生命值
        /// </summary>
        [Tooltip("最大生命值")] public float maxHp = 100; //最大生命值

        #endregion

        #region 移动相关

        /// <summary>
        ///     基础速度
        /// </summary>
        [Header("移动设置")] [Tooltip("基础速度")] public float baseSpeed;

        /// <summary>
        ///     当前速度
        /// </summary>
        [Tooltip("当前速度")] public float currentSpeed;

        /// <summary>
        ///     速度倍率
        /// </summary>
        [Tooltip("速度倍率")] public float speedMultiplier = 1.2f; //速度倍率

        #endregion

        #region 体力相关

        /// <summary>
        ///     体力
        /// </summary>
        [Header("体力设置")] [Tooltip("体力")] public float stamina = 100f;

        /// <summary>
        ///     最大体力
        /// </summary>
        [Tooltip("最大体力")] public float maxStamina = 100f; //体力上限

        /// <summary>
        ///     体力消耗速率
        /// </summary>
        [Tooltip("体力消耗速率")] public float staminaDrainRate = 10f; //体力消耗速率

        /// <summary>
        ///     体力恢复速率
        /// </summary>
        [Tooltip("体力恢复速率")] public float staminaRegenRate = 10f; //体力恢复速率

        #endregion

        #region 炸弹相关

        [Header("炸弹设置")] [Tooltip("炸弹数量")] public int bombCount;

        /// <summary>
        ///     炸弹伤害
        /// </summary>
        [Tooltip("炸弹伤害")] public float bombDamage = 20f; //爆炸伤害

        /// <summary>
        ///     炸弹爆炸范围
        /// </summary>
        [Tooltip("炸弹爆炸范围")] public int bombRadius = 5; //爆炸范围

        /// <summary>
        ///     炸弹爆炸时间
        /// </summary>
        [Tooltip("炸弹爆炸时间")] public float bombFuseTime = 3f; //爆炸时间

        /// <summary>
        ///     放置炸弹冷却时间
        /// </summary>
        [Tooltip("放置炸弹冷却时间")] public float bombCooldown = 0.8f; //放置炸弹冷却时间

        /// <summary>
        ///     炸弹恢复时间
        /// </summary>
        [Tooltip("炸弹恢复时间")] public float bombRecoveryTime = 2f;

        /// <summary>
        ///     最大炸弹数量
        /// </summary>
        [Tooltip("最大炸弹数量")] public int maxBombCount = 5; //最大炸弹数量

        /// <summary>
        ///     最大炸弹恢复时间
        /// </summary>
        [Tooltip("最大炸弹恢复时间")] public float maxBombRecoveryTime = 2f; //最大炸弹恢复时间

        /// <summary>
        ///     放置炸弹冷却时间
        /// </summary>
        [Tooltip("放置炸弹冷却时间")] public float maxBombCooldown = 0.8f; //放置炸弹冷却时间

        #endregion

        #region 状态相关

        [Header("角色状态")] public bool isDie;

        public bool isStaminaEmpty;

        #endregion

        # region 事件监听设置

        private void OnEnable()
        {
            GameEventSystem.AddListener<CharacterTakeDamageEvent>(OnTakeDamage);
            GameEventSystem.AddListener<CharacterDieEvent>(OnPlayerDie);
            GameEventSystem.AddListener<CharacterDieEvent>(OnKillPlayer);
            GameEventSystem.AddListener<ExpAddEvent>(OnExpAdd);
            GameEventSystem.AddListener<LeaveUpEvent>(OnLeaveUp);
            GameEventSystem.AddListener<GameOverEvent>(OnGameOver);
        }

        private void OnDisable()
        {
            GameEventSystem.RemoveListener<CharacterTakeDamageEvent>(OnTakeDamage);
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnPlayerDie);
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnKillPlayer);
            GameEventSystem.RemoveListener<ExpAddEvent>(OnExpAdd);
            GameEventSystem.RemoveListener<LeaveUpEvent>(OnLeaveUp);
            GameEventSystem.RemoveListener<GameOverEvent>(OnGameOver);
        }

        #endregion


        #region 事件监听

        private void OnLeaveUp(LeaveUpEvent evt)
        {
            if (isDie) return;
            if (evt.PlayerId == id)
            {
                print("玩家升级，当前等级:" + level);
                exp -= globalProper.maxExpToLevelUp;
                level++;

                //基础数值更新
                hp += characterProper.maxHpGrowth; // 生命值
                baseSpeed += characterProper.speedGrowth; // 速度
                //炸弹数值更新
                bombFuseTime = Mathf.Max(0.1f, bombFuseTime - characterProper.bombFuseTimeGrowth); // 修改：减少爆炸时间
                bombRadius += characterProper.bombRadiusGrowth; // 增加爆炸范围
                bombDamage += characterProper.bombDamageGrowth; // 增加炸弹伤害
                //更新炸弹冷却时间
                maxBombCooldown = Mathf.Max(0.1f, maxBombCooldown - characterProper.bombCooldownGrowth);

                //更新炸弹恢复时间
                maxBombRecoveryTime = Mathf.Max(0.5f, maxBombRecoveryTime - characterProper.bombRecoveryTimeGrowth);

                //最大值更新
                maxBombCount += characterProper.maxBombCountGrowth; // 炸弹数量
                maxStamina += characterProper.staminaGrowth; // 更新最大体力
                maxHp += characterProper.maxHpGrowth; // 最大生命值


                GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateSpeedEvent(id, baseSpeed));
                //更新UI


                GameEventSystem.Broadcast(new HUDEvent.LeaveUpEvent(id, hp, maxHp, stamina, maxStamina, exp, level,
                    globalProper.maxExpToLevelUp, currentSpeed, bombCount, maxBombCount, bombRecoveryTime, bombDamage,
                    bombRadius, bombFuseTime));
            }
            //TODO 其他玩家升级时界面更新
        }

        private void OnExpAdd(ExpAddEvent evt) //经验值增加事件
        {
            if (isDie) return;
            if (evt.PlayerId == id)
            {
                exp += evt.Exp;
                print($"{evt.PlayerId} 经验值增加 {evt.Exp} ,当前经验值 {exp}");

                if (level < characterProper.maxLevel && exp >= globalProper.maxExpToLevelUp)
                {
                    GameEventSystem.Broadcast(new LeaveUpEvent { PlayerId = evt.PlayerId });
                }
                else if (level >= characterProper.maxLevel)
                {
                    print($"{id}等级已满，无法升级");
                    exp = Mathf.Min(exp, globalProper.maxExpToLevelUp);
                    GameEventSystem.Broadcast(new HUDEvent.ExpAddEvent(id, exp, globalProper.maxExpToLevelUp));
                }
                else
                {
                    GameEventSystem.Broadcast(new HUDEvent.ExpAddEvent(id, exp, globalProper.maxExpToLevelUp));
                }
            }
        }


        private void OnKillPlayer(CharacterDieEvent evt) // 玩家死亡事件 击杀
        {
            if (isDie) return;
            if (evt.DieId != id && evt.AttackerID == id)
                GameEventSystem.Broadcast(new ExpAddEvent
                {
                    PlayerId = evt.AttackerID,
                    Exp = evt.Exp
                });
        }

        protected abstract void OnPlayerDie(CharacterDieEvent evt); //玩家死亡事件 死亡

        protected abstract void OnTakeDamage(CharacterTakeDamageEvent evt); // 玩家受伤事件


        protected void Die()
        {
            GameEventSystem.RemoveListener<CharacterTakeDamageEvent>(OnTakeDamage);
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnPlayerDie);
            GameEventSystem.RemoveListener<CharacterDieEvent>(OnKillPlayer);
            GameEventSystem.RemoveListener<ExpAddEvent>(OnExpAdd);
            GameEventSystem.RemoveListener<LeaveUpEvent>(OnLeaveUp);
            //TODO : 显示面板
        }

        private void OnGameOver(GameOverEvent evt)
        {
            //TODO 处理游戏结算
            if (GameModeSelect.NPCCount != 0)
            {
                if (isDie && evt.isWin) print("游戏结束，玩家失败");
                if (!isDie && evt.isWin) print("游戏结束，玩家胜利");
            }
            else
            {
                if (isDie)
                {
                    //游戏结束，输了
                }
            }
        }

        #endregion
    }
}