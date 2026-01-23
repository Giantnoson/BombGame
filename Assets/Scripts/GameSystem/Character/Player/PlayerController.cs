using System;
using config;
using GameSystem.EventSystem;
using GameSystem.GameScene.GameRuntimeScene;
using player;
// 引入GameSystem命名空间，可能包含游戏相关的系统类
using UnityEngine;
using UnityEngine.Serialization; // 引入Unity引擎的核心命名空间

namespace GameSystem.Character.Player
{
    public class PlayerController : BaseState
    {
        
        # region 变量定义
        [Header("控制器")]
        public PlayerMoveController moveController;
        public PlayerStateHUD playerStateHUD;

        public float moveVertical;
        public float moveHorizontal;

        public Vector3 moveDirection;
        public Vector3 beforeMoveDirection = Vector3.zero;
        
        public float cameraHorizontal;
        public float cameraVertical;
        public float beforeCameraHorizontal = 0f;
        public float beforeCameraVertical = 0f;
        public bool isStaminaUpdate = false;

        #endregion


        private void Start()
        {
            moveController.Init(this, characterId);
            InitHUD();
            print("HUD初始化成功");
        }

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

        #region 初始化函数

        public void PlayerControllerInit(string name, string id, CharacterType type, PlayerStateHUD hud)
        {
            characterName = name;
            characterId = id;
            characterType = type;
            playerStateHUD = hud;
            if (playerStateHUD == null)
            {
                Debug.LogError("playerStateHUD为空");
            }
            moveController = GetComponent<PlayerMoveController>();
            if (moveController == null)
            {
                Debug.LogError("moveController为空");
            }

            //严格按照此顺序初始化
            StateInit();
            print("玩家属性初始化成功");
        }
        


        private void InitHUD()
        {
            playerStateHUD.LoadHUD(characterId);
            GameEventSystem.Broadcast(new HUDEvent.LoadHUDEvent(characterId,characterName, characterType, characterProper, globalProper, hp, stamina, exp, level, currentSpeed));
        }

        #endregion
        
        
        private void Update()
        { 
            if(isDie) return;//如果玩家死亡，则不执行以下代码
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PutBomb();
            }
            StaminaUpdate();
            BombUpdate();
            MoveUpdate();
            CameraViewUpdate();
        }

        private void CameraViewUpdate()
        {
            cameraHorizontal = Input.GetAxis("Mouse X");
            cameraVertical = Input.GetAxis("Mouse Y"); ;
            if (cameraVertical == beforeCameraVertical && cameraHorizontal == beforeCameraHorizontal)
            {
                return;
            }
            GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateRotationXEvent(characterId, cameraHorizontal,  cameraVertical));
            beforeCameraHorizontal = cameraHorizontal;
            beforeCameraVertical = cameraVertical;
        }
        
        private void MoveUpdate()
        {
            moveHorizontal = Input.GetAxis("Horizontal"); // A/D
            moveVertical = Input.GetAxis("Vertical");     // W/S 
            moveDirection = new Vector3(moveHorizontal, 0, moveVertical);
            
            // 将移动向量从角色本地坐标系转换到世界坐标系
            moveDirection = transform.TransformDirection(moveDirection);
            if (moveDirection == beforeMoveDirection)
            {
                return;
            }
            GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateMoveDirectionEvent(characterId, moveDirection));
            beforeMoveDirection = moveDirection;
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
            GameEventSystem.Broadcast(new BombEvents.BombPlaceRequestEvent 
            { 
                Position = bombPos, 
                OwnerId = characterId,
                BombFuseTime = bombFuseTime,
                BombRadius = bombRadius,
                BombDamage = bombDamage 
            });
        }

        
        
        #region 事件监听
        private void OnLeaveUp(LeaveUpEvent evt)
        {
            if (evt.PlayerId == characterId)
            {
                print("玩家升级，当前等级:" + level);
                exp -= globalProper.maxExpToLevelUp;
                level++;
        
                //基础数值更新
                hp += characterProper.maxHpGrowth; // 生命值
                baseSpeed += characterProper.speedGrowth; // 速度
                //炸弹数值更新
                bombFuseTime = Mathf.Max(0.1f , bombFuseTime - characterProper.bombFuseTimeGrowth); // 修改：减少爆炸时间
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
                
                
                GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateSpeedEvent(characterId, baseSpeed));
                //更新UI

                
                GameEventSystem.Broadcast(new HUDEvent.LeaveUpEvent(characterId, hp,maxHp,stamina,maxStamina,exp,level,globalProper.maxExpToLevelUp,currentSpeed,bombCount,maxBombCount,bombRecoveryTime,bombDamage,bombRadius,bombFuseTime));
            }
            else
            {
                //TODO 其他玩家升级时界面更新
            }
        }

        private void OnExpAdd(ExpAddEvent evt) //经验值增加事件
        {
            
            if (evt.PlayerId == characterId)
            {
                exp += evt.Exp;
                print( $"{evt.PlayerId} 经验值增加 {evt.Exp} ,当前经验值 {exp}");
                
                if (level < characterProper.maxLevel && exp >= globalProper.maxExpToLevelUp)
                {
                    GameEventSystem.Broadcast(new LeaveUpEvent() { PlayerId = evt.PlayerId });
                }else if (level >= characterProper.maxLevel)
                {
                    print($"{characterId}等级已满，无法升级");
                    exp = Mathf.Min(exp, globalProper.maxExpToLevelUp);
                    GameEventSystem.Broadcast(new HUDEvent.ExpAddEvent(characterId, exp, globalProper.maxExpToLevelUp));
                }
                else
                {
                    GameEventSystem.Broadcast(new HUDEvent.ExpAddEvent(characterId, exp, globalProper.maxExpToLevelUp));
                }
                
            }
        }
        
        
        private void OnKillPlayer(CharacterDieEvent evt) // 玩家死亡事件 击杀
        {
            if (evt.DieId != characterId && evt.AttackerID == characterId)
            {
                GameEventSystem.Broadcast(new ExpAddEvent()
                {
                    PlayerId = evt.AttackerID,
                    Exp = evt.Exp
                });
            }
        }
        
        private void OnPlayerDie(CharacterDieEvent evt)//玩家死亡事件 死亡
        {
            if (evt.DieId == characterId)
            {
                isDie = true;
                characterId = "Die";
                Die();
                GetComponent<Collider>().enabled = false;
                Destroy(moveController);
            }
        }

        private void OnTakeDamage(CharacterTakeDamageEvent evt) // 玩家受伤事件
        {
            if (evt.HitId == characterId)
            {
                hp -= evt.Damage;
                print($"{evt.HitId} 受到来自 {evt.OwnerId}的 {evt.Damage} 伤害。剩余血量为: {hp}");//性能貌似没有之前print(evt.HitId+" 玩家受到来自 " + evt.OwnerId +" 伤害" + evt.Damage + " 剩余血量为: " + hp);高
                if (hp <= 0)//当玩家死亡时
                {
                    hp = 0;
                    print( characterId+ "玩家死亡");
                    GameEventSystem.Broadcast(new CharacterDieEvent()
                    {
                        AttackerID = evt.OwnerId,
                        DieId = characterId,
                        Exp = 50 * level
                    });
                    
                }
                GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(characterId, hp, maxHp));
            }
        }

        private void Die()
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
            if (GameModeSelect.IsEnableNPC)
            {
                if (isDie && evt.isWin)
                {
                    print("游戏结束，玩家失败");
                }
                if (!isDie && evt.isWin)
                {
                    print("游戏结束，玩家胜利");
                }
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