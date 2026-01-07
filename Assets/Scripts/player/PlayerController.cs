using System;
using Game_props; // 引入System命名空间，提供基础类和基类
using GameSystem;
using UnityEditor; // 引入GameSystem命名空间，可能包含游戏相关的系统类
using UnityEngine;                        // 引入Unity引擎的核心命名空间
using UnityEngine.PlayerLoop;            // 引入Unity的玩家循环命名空间，用于控制游戏逻辑更新顺序
using UnityEngine.Serialization;         // 引入Unity的序列化命名空间，用于对象序列化和反序列化

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
        [Tooltip("炸弹模块")]
        public Bomb bomb;
        [Tooltip("炸弹预制体")]
        public GameObject bombPrefab;
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
        [Tooltip("当前速度")]
        public float currentSpeed;

        [Header("控制器")] public PlayerMoveController moveController;

        #endregion
        
        # region 事件监听设置
        private void OnEnable()
        {
            EventSystem.AddListener<PlayerTakeDamageEvent>(OnTakeDamage);
            EventSystem.AddListener<PlayerDieEvent>(OnPlayerDie);
            EventSystem.AddListener<PlayerDieEvent>(OnKillPlayer);
            EventSystem.AddListener<ExpAddEvent>(OnExpAdd);
        }

        private void OnDisable()
        {
            EventSystem.RemoveListener<PlayerTakeDamageEvent>(OnTakeDamage);
            EventSystem.RemoveListener<PlayerDieEvent>(OnPlayerDie);
            EventSystem.RemoveListener<PlayerDieEvent>(OnKillPlayer);
            EventSystem.RemoveListener<ExpAddEvent>(OnExpAdd);

        }
        #endregion

        #region 初始化
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
            if (bombPrefab == null)
            {
                Debug.LogError("bomb为空");
            }
            else
            {
                print("bomb加载成功");
                if (!PrefabUtility.IsPartOfAnyPrefab(bombPrefab))
                {
                    Debug.LogWarning("bomb字段不是预制体引用，请检查设置");
                }

                bomb = bombPrefab.GetComponent<Bomb>();
                if (bomb == null)
                {
                    Debug.LogError("bomb预制体上没有Bomb组件"); 
                }
            }
            InitProper();
            LoadProper();
            print("玩家初始化成功");
        }
        
        private void LoadProper()
        {
            //初始值设定
            hp = playerProper.maxHp; // 初始化生命值
            currentSpeed = playerProper.speed; // 初始化速度
            bombCount = globalProper.initBombCount; // 初始化炸弹数量
            level = globalProper.initLevel;// 初始化等级
            exp = globalProper.initExp;// 初始化经验值
            //数值传递
            moveController.MoveSpeed = currentSpeed;
            print("成功加载玩家属性");
            bomb.ownerId = playerId;
            bomb.bombFuseTime = playerProper.bombFuseTime;
            bomb.bombRadius = playerProper.bombRadius;
            bomb.bombDamage = playerProper.bombDamage;
            print("成功加载炸弹属性");
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
                position = bombPos, 
                bombPrefab = bombPrefab,
                ownerId = playerId 
            });
        }

        private void LeaveUp()
        {
            print("玩家升级，当前等级:" + level);
            exp -= globalProper.maxExpToLevelUp;
            level++;
            
            //基础数值更新
            hp += playerProper.maxHpGrowth; // 生命值
            currentSpeed += playerProper.speedGrowth; // 速度
            moveController.MoveSpeed = currentSpeed;  
            bombCount += playerProper.maxBombCountGrowth; // 炸弹数量
            
            //炸弹数值更新
            bomb.bombFuseTime += playerProper.bombFuseTimeGrowth;
            bomb.bombRadius += playerProper.bombRadiusGrowth;
            bomb.bombDamage += playerProper.bombDamageGrowth;
        }

        #region 事件监听函数
        private void OnPlayerDie(PlayerDieEvent evt)//玩家死亡事件 死亡
        {
            if (evt.DieId == playerId)
            {
                Destroy(gameObject);//销毁玩家
            }
        }

        private void OnExpAdd(ExpAddEvent evt)
        {
            
            if (evt.PlayerId == playerId && level < playerProper.maxLevel)
            {
                exp += evt.Exp;
                print( evt.PlayerId + "玩家经验值增加" + evt.Exp + "当前经验值" + exp);
                if (level < playerProper.maxLevel && exp >= globalProper.maxExpToLevelUp)
                {
                    LeaveUp();
                }
            }
        }
        
        private void OnKillPlayer(PlayerDieEvent evt) // 玩家死亡事件 击杀
        {
            if (evt.DieId != playerId && evt.AttackerID == playerId)
            {
                exp += evt.Exp;
                if (level < playerProper.maxLevel && exp >= globalProper.maxExpToLevelUp)
                {
                    LeaveUp();
                }
            }
        }

        private void OnTakeDamage(PlayerTakeDamageEvent evt) // 玩家受伤事件
        {
            if (evt.HitId == playerId)
            {

                hp -= evt.Damage;
                print(evt.HitId+" 玩家受到来自 " + evt.OwnerId +" 伤害" + evt.Damage + " 剩余血量为: " + hp);
                if (hp <= 0)//当玩家死亡时
                {
                    print( playerId+"玩家死亡");
                    EventSystem.Broadcast(new PlayerDieEvent()
                    {
                        AttackerID = evt.OwnerId,
                        DieId = playerId,
                        Exp = 50 * level
                    });
                }
            }
        }
        #endregion
        
    }
}