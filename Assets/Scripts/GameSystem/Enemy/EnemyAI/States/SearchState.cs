
using UnityEngine;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 搜索状态类，继承自EnemyAIBaseState，用于控制敌人的搜索行为
    /// </summary>
    public class SearchState : EnemyAIBaseState
    {
        // 目标位置
        private Vector3 targetPosition;
        // 是否有目标
        private bool hasTarget;
        // 目标检查间隔时间（秒）
        private float targetCheckInterval = 0.2f;
        // 上次检查时间
        private float lastCheckTime;

        /// <summary>
        /// 进入状态时的回调函数
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入搜索状态");
            hasTarget = false;
            FindNewTarget();
        }
        /// <summary>
        /// 状态更新时的回调函数
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        /// <param name="elapseSeconds">距离上一帧的时间（秒）</param>
        /// <param name="realElapseSeconds">距离状态进入的时间（秒）</param>
        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds, float realElapseSeconds)
        {

            // 定期检查状态
            if (Time.time - lastCheckTime >= targetCheckInterval)
            {
                lastCheckTime = Time.time;
                CheckState(fsm);
            }

            // 移动向目标
            if (hasTarget)
            {
                MoveToTarget();
            }
        }
        /// <summary>
        /// 离开状态时的回调函数
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        /// <param name="isShutdown">是否是关闭状态机</param>
        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {

            Debug.Log("离开搜索状态");
            Owner.StopMove();
        }

        /// <summary>
        /// 检查当前状态，决定是否需要切换到其他状态
        /// </summary>
        ///<param name="fsm">有限状态机引用</param>
        private void CheckState(IFsm<EnemyAIController> fsm)
        {
            // 1. 检测爆炸威胁（最高优先级）

            if (Owner.IsInExplosionRange(Owner.transform.position))
            {
                ChangeState<AvoidExplosionState>(fsm);
                return;
            }

            // 2. 检测玩家（高优先级）
            Transform nearestPlayer = Owner.GetNearestPlayer();
            if (nearestPlayer != null)
            {
                float distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                if (distance <= Owner.chaseRange)
                {
                    ChangeState<ChasePlayerState>(fsm);
                    return;
                }
            }

            // 3. 检查是否到达目标
            if (hasTarget && Vector3.Distance(Owner.transform.position, targetPosition) < 0.5f)
            {
                ChangeState<PlaceBombState>(fsm);
                return;
            }

            // 4. 检查路径是否被爆炸阻挡
            if (IsPathBlockedByExplosion())
            {
                ChangeState<PathWaitState>(fsm);
                return;
            }
        }

        /// <summary>
        /// 寻找新目标
        /// </summary>
        private void FindNewTarget()
        {
            // TODO: 实现寻找最近可破坏方块的逻辑
            // 临时：随机选择一个位置
            targetPosition = Owner.transform.position + Random.insideUnitSphere * 5f;
            targetPosition.y = Owner.transform.position.y;
            hasTarget = true;
        }

        /// <summary>
        /// 移动向目标
        /// </summary>
        private void MoveToTarget()
        {
            Owner.MoveTo(targetPosition);
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
