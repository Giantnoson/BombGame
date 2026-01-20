
using System;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.EventSystem;
using Game_props;
using GameSystem.Map;
using player;
using UnityEditor.SceneManagement;
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
        public int maxBombCount = 100;

        [Tooltip("炸弹冷却时间")]
        public float bombCooldown = 3f;

        [Tooltip("炸弹爆炸范围")]
        public float bombRadius = 3f;

        [Tooltip("炸弹伤害")]
        public float bombDamage = 20f;

        [Tooltip("炸弹爆炸时间")]
        public float bombFuseTime = 3f;
        
        [Tooltip("炸弹数量冷却")]
        public float bombCountColdDown = 3f;

        [Header("状态")]
        [Tooltip("敌人ID")]
        public int _enemyId = -1;
        
        public int EnemyId => _enemyId;

        public float stoppingDistance = 1f;
        
        [Tooltip("血量")]
        public float hp = 100f;
        [Tooltip("当前炸弹数量")]
        public int currentBombCount = 100;

        [Tooltip("当前炸弹冷却")]
        public float currentBombCooldown = 0f;
        [Tooltip("当前炸弹数量冷却")] 
        public float currentBombCountColdDown = 3f;

        [Tooltip("状态记录")]
        public List<EnemyAIStates> StatusLog;
        [Tooltip("状态队列")]
        public Queue<EnemyAIStates> StatusQueue = new Queue<EnemyAIStates>();
        
        public bool isMoving;


        
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
        public NavMeshAgent enemyAgent;

        public BombPos bombPos;
        
        
        
        GameObject[] players;
        
        
        // 炸弹位置
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
            }
            mapScan = FindAnyObjectByType<MapScan>();
            enemyAgent = GetComponent<NavMeshAgent>();
            bombPos = FindAnyObjectByType<BombPos>();
            if (mapScan == null)
            {
                Debug.LogError("无法找到地图扫描组件");
            }

            if (enemyAgent == null)
            {
                Debug.LogError("无法找到NavMeshAgent组件");
            }

            if (bombPos == null)
            {
                Debug.LogError("无法找到BombPos组件");
            }
            enemyAgent.stoppingDistance = 0f;
        }


        private void Start()
        {
            // 初始化FSM
            isMoving = false;
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
            StatusLog.Add(EnemyAIStates.Idle);
            StatusQueue.Enqueue(EnemyAIStates.Idle);//预存入两个，方便启动
            StatusQueue.Enqueue(EnemyAIStates.Idle);
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

            if (currentBombCountColdDown > 0)
            {
                currentBombCountColdDown = Mathf.Max(0f, currentBombCountColdDown - Time.deltaTime);
            }else if (currentBombCountColdDown == 0)
            {
                currentBombCountColdDown = bombCountColdDown;
                currentBombCount ++;
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
            return bombPos.BombExportArea.ContainsKey(ToBombPutPos(position));
        }

        /*public bool CheckBombCanAttack(BombPlaceRequestEvent bombInfo)
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
        }*/

        public Vector3 findSafePosition(Vector3 pos)
        {
            mapScan.ScanArea(pos, Mathf.CeilToInt(detectionRange));
            var directions = new List<Vector3> { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            if (!mapScan.IsValidPosition(pos))
            {
                Debug.LogError("不合法的位置: " + pos);
                return Vector3.down;
            }

            pos = ToBombPutPos(pos);
            var que = new Queue<Vector3>();
            que.Enqueue(pos);
            var visited = new HashSet<Vector3>();
            visited.Add(pos);

            while (que.Count > 0)
            {
                var current = que.Dequeue();
                foreach (var direction in directions)
                {
                    var nextPos = current + direction;
                    if (!visited.Contains(nextPos) && mapScan.IsWalkable(nextPos))
                    {
                        visited.Add(nextPos);
                        que.Enqueue(nextPos);
                        if (!IsInExplosionRange(nextPos))
                        {
                            return mapScan.GetRealCoord(mapScan.GetVirtualCoord(nextPos));
                        }
                    }
                }
            }
            return Vector3.down;
        }





        public Vector3 ToBombPutPos(Vector3 pos)
        {
            pos.x = Mathf.Ceil(pos.x) - 0.5f;
            pos.z = Mathf.Ceil(pos.z) - 0.5f;
            pos.y = 0f;
            return pos;
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
                OwnerId = EnemyId,
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
        public bool MoveTo(Vector3 targetPosition)
        {
            //
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                //设置目的地
                isMoving = enemyAgent.SetDestination(targetPosition);
                return isMoving;

            }

            isMoving = false;
            return false;
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMove()
        {
            isMoving = false;
            enemyAgent.SetDestination(transform.position);
        }

        #endregion
        
        
        
    }
}
