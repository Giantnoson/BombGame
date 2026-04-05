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

            // 2. 检查玩家是否存在
            if (targetPlayer == null)
            {
                // 没有玩家，切换到搜索状态
                ChangeState<SearchState>(fsm);
                return;
            }

            // 3. 检查是否到达攻击范围
            targetPlayer = Owner.GetNearestPlayer();
            if (targetPlayer != null)
            {
                var distance = Vector3.Distance(Owner.transform.position, Owner.ToBombPutPos(targetPlayer.position));
                if (distance <= Owner.stoppingDistance + 0.3f)
                {
                    ChangeState<AttackState>(fsm);
                    return;
                }

                Owner.MoveTo(Owner.MapInfo.SearchPath(Owner.transform.position,
                    Owner.ToBombPutPos(targetPlayer.position), true));

                // 4. 检查是否超出追击范围
                if (distance > Owner.chaseRange)
                {
                    // 超出范围，切换到搜索状态
                    ChangeState<SearchState>(fsm);
                    return;
                }
            }

            // 5. 检查路径是否被爆炸阻挡
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

            if (isMoving) return;
            if (!Owner.MoveTo(Owner.MapInfo.SearchPath(Owner.transform.position,
                    Owner.ToBombPutPos(targetPlayer.position), true)))
                ChangeState<SearchState>(fsm);
            else
                isMoving = true;
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