using GameSystem.Character.Enemy.Fsm;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI.States
{
    /// <summary>
    ///     追击玩家状态 - 追击并接近玩家
    /// </summary>
    public class ChasePlayerState : EnemyAIBaseState
    {
        private readonly float targetCheckInterval = 0.1f;
        private bool isMoving;
        private float lastCheckTime;
        private Transform targetPlayer;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入追击玩家状态");
            Owner.statusLog.Add(EnemyAIStates.ChasePlayer);
            Owner.StatusQueue.Enqueue(EnemyAIStates.ChasePlayer);
            Owner.StatusQueue.Dequeue();
            targetPlayer = Owner.GetNearestPlayer();
            /*
            Owner.enemyAgent.stoppingDistance = Owner.stoppingDistance;
            */
            isMoving = false;
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            // 定期检查状态
            if (Time.time - lastCheckTime >= targetCheckInterval)
            {
                lastCheckTime = Time.time;
                CheckState(fsm);
            }

            // 追击玩家
            if (targetPlayer != null)
            {
                targetPlayer = Owner.GetNearestPlayer();
                ChasePlayer();
            }
            
            // 检测移动是否已完成，重置isMoving标志
            if (isMoving && !Owner.isMoving)
                isMoving = false;
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开追击玩家状态");
            targetPlayer = null;
            Owner.StopMove();
            /*
            Owner.enemyAgent.stoppingDistance = 0f;
            */
            isMoving = false;
        }

        /// <summary>
        ///     检查当前状态
        /// </summary>
        private void CheckState(IFsm<EnemyAIController> fsm)
        {
            // 1. 检测爆炸威胁（最高优先级）
            if (Owner.IsInExplosionRange(Owner.transform.position))
            {
                ChangeState<AvoidExplosionState>(fsm);
                return;
            }

            // 2. 刷新目标玩家并检查是否存在（地图同步：玩家可能已死亡或离开）
            targetPlayer = Owner.GetNearestPlayer();
            if (targetPlayer == null)
            {
                // 没有活着的玩家，切换到搜索状态
                ChangeState<SearchState>(fsm);
                return;
            }

            // 3. 检查是否到达攻击范围
            if (targetPlayer != null)
            {
                var distance = Vector3.Distance(Owner.transform.position, Owner.ToBombPutPos(targetPlayer.position));
                if (distance <= Owner.stoppingDistance + 0.3f)
                {
                    ChangeState<AttackState>(fsm);
                    return;
                }

                // 4. 检查是否超出追击范围（不在此处发起移动，由ChasePlayer统一处理）
                if (distance > Owner.chaseRange)
                {
                    // 超出范围，切换到搜索状态
                    ChangeState<SearchState>(fsm);
                    return;
                }

                // 5. 验证路径是否可达
                var path = Owner.MapInfo.SearchPath(Owner.transform.position,
                    Owner.ToBombPutPos(targetPlayer.position), true);
                if (path == null)
                {
                    // 路径不可达，切换到搜索状态
                    ChangeState<SearchState>(fsm);
                    return;
                }
            }

            // 6. 检查路径是否被爆炸阻挡
            if (IsPathBlockedByExplosion()) ChangeState<PathWaitState>(fsm);
        }

        /// <summary>
        ///     追击玩家
        /// </summary>
        private void ChasePlayer()
        {
            if (targetPlayer == null)
            {
                isMoving = false;
                return;
            }

            // 如果正在移动中，等待移动完成（由OnUpdate中的检测重置isMoving）
            if (isMoving) return;
            
            var path = Owner.MapInfo.SearchPath(Owner.transform.position,
                Owner.ToBombPutPos(targetPlayer.position), true);
            if (path == null || !Owner.MoveTo(path))
            {
                // 路径无效或移动失败，切换到搜索状态
                ChangeState<SearchState>(fsm);
            }
            else
            {
                isMoving = true;
            }
        }

        /// <summary>
        ///     检查路径是否被爆炸阻挡
        /// </summary>
        private bool IsPathBlockedByExplosion()
        {
            if (Owner.IsInExplosionRange(Owner.transform.position)) return true;
            return false;
        }
    }
}