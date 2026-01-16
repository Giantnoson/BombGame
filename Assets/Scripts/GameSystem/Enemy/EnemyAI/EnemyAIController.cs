
using System;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.EventSystem;
using Game_props;
using GameSystem.Map;
using player;
using UnityEngine.AI;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 敌人AI控制器
    /// </summary>
    public class EnemyAIController : MonoBehaviour
    {
        [Header("AI设置")]
        [Tooltip("检测范围")]
        public float detectionRange = 8f;

        [Tooltip("追击范围")]
        public float chaseRange = 8f;

        [Tooltip("攻击范围")]
        public float attackRange = 3f;

        [Tooltip("移动速度")]
        public float moveSpeed = 3f;

        [Header("炸弹设置")]
        [Tooltip("最大炸弹数量")]
        public int maxBombCount = 3;

        [Tooltip("炸弹冷却时间")]
        public float bombCooldown = 3f;

        [Tooltip("炸弹爆炸范围")]
        public float bombRadius = 3f;

        [Tooltip("炸弹伤害")]
        public float bombDamage = 20f;

        [Tooltip("炸弹爆炸时间")]
        public float bombFuseTime = 3f;

        [Header("状态")]
        [Tooltip("敌人ID")]
        private int _enemyId = -1;
        
        public int EnemyId => _enemyId;
        
        [Tooltip("血量")]
        public float hp = 100f;
        [Tooltip("当前炸弹数量")]
        public int currentBombCount = 1;

        [Tooltip("当前炸弹冷却")]
        public float currentBombCooldown = 0f;

        [Tooltip("是否死亡")]
        public bool isDead = false;

        // FSM引用
        private Fsm<EnemyAIController> fsm;

        // 目标引用
        private Transform targetPlayer;

        // 组件引用
        private CharacterController characterController;

        // 地图扫描组件
        public MapScan mapScan;
        // 敌人寻路组件
        private NavMeshAgent _enemyAgent;
        
        GameObject[] players;
        
        
        // 炸弹位置
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
            }
            mapScan = GetComponent<MapScan>();
            _enemyAgent = GetComponent<NavMeshAgent>();
            if (mapScan == null)
            {
                Debug.LogError("无法找到地图扫描组件");
            }

            if (_enemyAgent == null)
            {
                Debug.LogError("无法找到NavMeshAgent组件");
            }

        }


        private void Start()
        {
            // 初始化FSM
            InitializeFSM();
            players = GameObject.FindGameObjectsWithTag("Player");

        }

        private void InitializeFSM()
        {
            // 创建所有状态
            var idleState = new IdleState();
            var searchState = new SearchState();
            var pathWaitState = new PathWaitState();
            var chasePlayerState = new ChasePlayerState();
            var placeBombState = new PlaceBombState();
            var avoidExplosionState = new AvoidExplosionState();
            var attackState = new AttackState();

            // 创建FSM
            fsm = Fsm<EnemyAIController>.Create(
                "EnemyAI",
                this,
                idleState,
                searchState,
                pathWaitState,
                chasePlayerState,
                placeBombState,
                avoidExplosionState,
                attackState
            );

            // 启动FSM，从Idle状态开始
            fsm.Start<IdleState>();
        }

        private void Update()
        {
            if (isDead) return;

            // 更新炸弹冷却
            if (currentBombCooldown > 0)
            {
                currentBombCooldown = Mathf.Max(0f, currentBombCooldown - Time.deltaTime);
            }

            // 更新FSM
            if (fsm != null && fsm.IsRunning)
            {
                fsm.Update(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        private void OnDestroy()
        {
            // 清理FSM
            if (fsm != null)
            {
                fsm.Clear();
            }
        }

        #region 公共方法

        /// <summary>
        /// 获取最近的玩家
        /// </summary>
        public Transform GetNearestPlayer()
        {
            // 查找所有玩家
            Transform nearest = null;
            float minDistance = Mathf.Infinity;

            foreach (var player in players)
            {

                if (player.GetComponent<PlayerController>()?.isDie == false)
                {
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (distance < minDistance && distance <= detectionRange)
                    {
                        minDistance = distance;
                        nearest = player.transform;
                    }
                }
            }
            return nearest;
        }

        /// <summary>
        /// 检测是否在爆炸范围内
        /// </summary>
        public bool IsInExplosionRange(Vector3 position)
        {
            foreach (KeyValuePair<Vector3,BombPlaceRequestEvent> bombPoss in BombPos.Instance.BombInfo)
            {
                if (Vector3.Distance(position, bombPoss.Key) <= bombPoss.Value.BombRadius)
                {
                    mapScan.ScanArea(transform.position, Mathf.CeilToInt(bombPoss.Value.BombRadius) + 1);
                    if (mapScan.TagPointPairsMap.ContainsKey(ObjectType.Bomb.ToString()))
                    {
                        return CheckBombCanAttack(bombPoss.Value);
                    }
                    else
                    {
                        
                        Debug.LogError("地图扫描器未扫描炸弹");
                    }


                }
            }
            return false;
        }

        public bool CheckBombCanAttack(BombPlaceRequestEvent bombInfo)
        {
            Vector2Int enemyVirtualCoord = mapScan.GetVirtualCoord(transform.position);//获取NPC虚拟坐标
            Vector2Int bombVirtualCoord = mapScan.GetVirtualCoord(bombInfo.Position);//获取炸弹虚拟坐标
            if (enemyVirtualCoord.x != bombVirtualCoord.x && enemyVirtualCoord.y != bombVirtualCoord.y) //如果NPC坐标与炸弹坐标没有一个重合
                return false;
            char wall = mapScan.TagPointPairsMap[ObjectType.Wall.ToString()];// 获取墙壁标签
            int radius = Mathf.CeilToInt(bombInfo.BombRadius);//获取炸弹半径
            int offsetDistance = mapScan.offsetDistance; //获取地图偏移距离
            if (enemyVirtualCoord.x == bombVirtualCoord.x)//如果NPC坐标与炸弹坐标x重合，则对Y进行爆炸检测
            {
                for (int i = bombVirtualCoord.y + 1; i < bombVirtualCoord.y + radius - 1 && i < offsetDistance * 2; i++)
                {
                    if (enemyVirtualCoord.y == i)
                    {
                        return true;
                    }
                    if (mapScan.MapData.Map[bombVirtualCoord.x, i] == wall)
                    {
                        break;
                    }
                }
                for (int i = bombVirtualCoord.y - 1; i > bombVirtualCoord.y - radius + 1 && i >= 0; i--)
                {
                    if (enemyVirtualCoord.y == i)
                    {
                        return true;
                    }
                    if (mapScan.MapData.Map[bombVirtualCoord.x, i] == wall)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (int i = bombVirtualCoord.x + 1; i < bombVirtualCoord.x + radius - 1 && i < offsetDistance * 2; i++)
                {
                    if (enemyVirtualCoord.x == i)
                    {
                        return true;
                    }
                    if (mapScan.MapData.Map[i, bombVirtualCoord.y] == wall)
                    {
                        break;
                    }
                }
                for (int i = bombVirtualCoord.x - 1; i > bombVirtualCoord.x - radius + 1 && i >= 0; i--)
                {
                    if (enemyVirtualCoord.x == i)
                    {
                        return true;
                    }
                    if (mapScan.MapData.Map[i, bombVirtualCoord.y] == wall)
                    {
                        break;
                    }
                }
            }
            return false;
        }
        
        
        
        
        
        
        
        
        /// <summary>
        /// 放置炸弹
        /// </summary>
        public void PlaceBomb()
        {
            if (currentBombCount <= 0 || currentBombCooldown > 0) return;

            Vector3 bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0f;

            // 广播炸弹放置事件
            GameEventSystem.Broadcast(new BombPlaceRequestEvent
            {
                Position = bombPos,
                OwnerId = gameObject.GetInstanceID(),
                BombFuseTime = bombFuseTime,
                BombRadius = bombRadius,
                BombDamage = bombDamage
            });

            currentBombCount--;
            currentBombCooldown = bombCooldown;
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public void MoveTo(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                characterController.SimpleMove(direction * moveSpeed);
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMove()
        {
            characterController.SimpleMove(Vector3.zero);
        }

        #endregion
        
        
    }
}
