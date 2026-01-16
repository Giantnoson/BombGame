
using UnityEngine;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 追击玩家状态 - 追击并接近玩家
    /// </summary>
    public class ChasePlayerState : EnemyAIBaseState
    {
        private Transform targetPlayer;
        private float targetCheckInterval = 0.1f;
        private float lastCheckTime;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入追击玩家状态");
            targetPlayer = Owner.GetNearestPlayer();
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds, float realElapseSeconds)
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
                ChasePlayer();
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开追击玩家状态");
            Owner.StopMove();
        }

        /// <summary>
        /// 检查当前状态
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
                targetPlayer = Owner.GetNearestPlayer();
                if (targetPlayer == null)
                {
                    // 没有玩家，切换到搜索状态
                    ChangeState<SearchState>(fsm);
                    return;
                }
            }

            // 3. 检查是否到达攻击范围
            float distance = Vector3.Distance(Owner.transform.position, targetPlayer.position);
            if (distance <= Owner.attackRange)
            {
                ChangeState<AttackState>(fsm);
                return;
            }

            // 4. 检查是否超出追击范围
            if (distance > Owner.chaseRange)
            {
                // 超出范围，切换到搜索状态
                ChangeState<SearchState>(fsm);
                return;
            }

            // 5. 检查路径是否被爆炸阻挡
            if (IsPathBlockedByExplosion())
            {
                ChangeState<PathWaitState>(fsm);
                return;
            }
        }

        /// <summary>
        /// 追击玩家
        /// </summary>
        private void ChasePlayer()
        {
            if (targetPlayer == null) return;

            // 保持安全距离
            Vector3 direction = (targetPlayer.position - Owner.transform.position).normalized;
            Vector3 targetPos = Owner.transform.position + direction * (Owner.attackRange - 0.5f);
            targetPos.y = Owner.transform.position.y;

            Owner.MoveTo(targetPos);
        }

        /// <summary>
        /// 检查路径是否被爆炸阻挡
        /// </summary>
        private bool IsPathBlockedByExplosion()
        {
            // TODO: 实现路径爆炸检测
            return false;
        }
    }
}
