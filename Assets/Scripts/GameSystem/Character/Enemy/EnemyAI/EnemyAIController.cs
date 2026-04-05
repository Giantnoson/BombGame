using System;
using System.Collections.Generic;
using Config;
using GameSystem.Character.common;
using GameSystem.Character.Enemy.EnemyAI.States;
using GameSystem.Character.Enemy.Fsm;
using GameSystem.Character.Player;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameProps;
using GameSystem.Map;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI
{
    /// <summary>
    ///     敌人AI控制器
    /// </summary>
    public class EnemyAIController : BaseState
    {
        [Header("AI设置")] [Tooltip("检测范围")] public float detectionRange = 8f;

        [Tooltip("追击范围")] public float chaseRange = 8f;

        [Tooltip("攻击范围")] public float attackRange = 3f;

        [Header("状态")] public float stoppingDistance = 0.3f;

        [Tooltip("状态记录")] public List<EnemyAIStates> statusLog = new List<EnemyAIStates>();

        public bool isMoving;


        [Tooltip("是否死亡")] public bool isDead;

        // 敌人寻路组件
        /*
        public NavMeshAgent enemyAgent;
        */

        //炸弹位置组件
        public BombPos bombPos;

        //地图信息组件
        public MapInfo MapInfo;

        // 组件引用
        private CharacterController characterController;

        // FSM引用
        private Fsm<EnemyAIController> fsm;

        private GameObject[] players;

        [Tooltip("状态队列")] public Queue<EnemyAIStates> StatusQueue = new();

        // 目标引用
        private Transform targetPlayer;

        public EnemyMoveController MoveController { get; private set; }


        // 炸弹位置

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null) characterController = gameObject.AddComponent<CharacterController>();
            
            // 初始化状态记录列表
            if (statusLog == null) statusLog = new List<EnemyAIStates>();
            /*
            enemyAgent = GetComponent<NavMeshAgent>();
            */
            bombPos = FindAnyObjectByType<BombPos>();
            MapInfo = MapInfo.Instance;
            MoveController = GetComponent<EnemyMoveController>();
            /*if (enemyAgent == null)
            {
                Debug.LogError("无法找到NavMeshAgent组件");
            }*/

            if (bombPos == null) Debug.LogError("无法找到BombPos组件");

            if (MapInfo == null) Debug.LogError("无法找到MapInfo组件");

            if (MoveController == null) Debug.LogError("无法找到EnemyMoveController组件");

            MoveController.Init(this, characterController);

            /*
            enemyAgent.stoppingDistance = 0f;
        */
        }

        public void EmenyControllerInit(string name, string id, CharacterType type)
        {
            characterName = name;
            this.id = id;
            characterType = type;
            foreach (var cam in gameObject.GetComponentsInChildren<Camera>())
            {
                cam.gameObject.SetActive(false);
            }
            //严格按照此顺序初始化
            StateInit();
            print("玩家属性初始化成功");
        }


        private void Start()
        {
            // 初始化FSM
            isMoving = false;
            InitializeFSM();
            players = GameObject.FindGameObjectsWithTag("Player");
        }

        private void Update()
        {
            if (isDead) return;

            // 定期更新玩家列表（例如每秒更新一次，或者在获取最近玩家时更新）
            if (Time.frameCount % 60 == 0)
            {
                players = GameObject.FindGameObjectsWithTag("Player");
            }

            StaminaUpdate();
            BombUpdate();
            // 更新FSM
            if (fsm != null && fsm.IsRunning) fsm.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            // 清理FSM
            if (fsm != null) fsm.Clear();
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
            statusLog.Add(EnemyAIStates.Idle);
            StatusQueue.Enqueue(EnemyAIStates.Idle); //预存入两个，方便启动
            StatusQueue.Enqueue(EnemyAIStates.Idle);
            fsm.Start<IdleState>();
        }


        protected override void OnPlayerDie(CharacterDieEvent evt)
        {
            if (isDie) return;
            if (evt.DieId == id)
            {
                isDie = true;
                id = "Die";
                fsm.Clear();
                Die();
            }
        }

        protected override void OnTakeDamage(CharacterTakeDamageEvent evt)
        {
            if (isDie) return;
            if (evt.HitId == id)
            {
                hp -= evt.Damage;
                print($"{evt.HitId} 受到来自 {evt.Id}的 {evt.Damage} 伤害。剩余血量为: {hp}");
                if (hp <= 0) //当玩家死亡时
                {
                    hp = 0;
                    print(id + "玩家死亡");
                    GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
                    GameEventSystem.Broadcast(new CharacterDieEvent
                    {
                        AttackerID = evt.Id,
                        DieId = id,
                        Exp = 50 * level
                    });
                    return;
                }

                GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
            }
        }

        #region 公共方法

        /// <summary>
        ///     获取最近的玩家
        /// </summary>
        public Transform GetNearestPlayer()
        {
            // 查找所有玩家
            Transform nearest = null;
            var minDistance = Mathf.Infinity;

            foreach (var player in players)
                if (player.GetComponent<PlayerController>()?.isDie == false)
                {
                    var distance = Vector3.Distance(transform.position, player.transform.position);
                    if (distance < minDistance && distance <= detectionRange)
                    {
                        minDistance = distance;
                        nearest = player.transform;
                    }
                }

            return nearest;
        }

        /// <summary>
        ///     检测是否在爆炸范围内
        /// </summary>
        public bool IsInExplosionRange(Vector3 position)
        {
            return bombPos.BombExportArea.ContainsKey(ToBombPutPos(position));
        }

        public Vector3 findSafePosition(Vector3 pos)
        {
            var directions = new List<Vector3> { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            if (!MapInfo.IsValidPosition(pos))
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
                    if (!visited.Contains(nextPos) && MapInfo.IsWalkable(nextPos))
                    {
                        visited.Add(nextPos);
                        que.Enqueue(nextPos);
                        if (!IsInExplosionRange(nextPos)) return MapInfo.GetRealCoord(MapInfo.GetVirtualCoord(nextPos));
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

        public Vector3 ToSearchPos(Vector3 pos)
        {
            pos.y = 0.5f;
            return pos;
        }


        /// <summary>
        ///     放置炸弹
        /// </summary>
        public void PutBomb(Action<bool> callBack)
        {
            if (bombCooldown > 0 || bombCount == 0)
            {
                print("炸弹冷却或数量为0，放置失败");
                return;
            }

            var bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0.5f;
            var hitColliders = Physics.OverlapBox(bombPos, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity);
            if (hitColliders.Length > 0)
                foreach (var collider1 in hitColliders)
                    if (!collider1.gameObject.CompareTag(tag))
                    {
                        print("炸弹放置失败，位置有障碍物");
                        return;
                    }

            bombCooldown = maxBombCooldown;
            bombCount--;
            bombPos.y = 0f;
            print("炸弹放置位置:" + bombPos);
            GameEventSystem.Broadcast(new BombEvents.BombPlaceRequestEvent
            {
                Position = bombPos,
                Id = id,
                BombFuseTime = bombFuseTime,
                BombRadius = bombRadius,
                BombDamage = bombDamage,
                CallBack = callBack
            });
        }


        /// <summary>
        ///     移动到目标位置
        /// </summary>
        /*
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
        */
        public bool MoveTo(PathInfo path, bool isEnterSafeMode = true)
        {
            isMoving = true;
            return MoveController.MoveToTarget(path, isEnterSafeMode);
        }

        /// <summary>
        ///     停止移动
        /// </summary>
        public void StopMove()
        {
            isMoving = false;
            /*
            enemyAgent.SetDestination(transform.position);
            */
            MoveController.StopMoving();
        }

        #endregion
    }
}