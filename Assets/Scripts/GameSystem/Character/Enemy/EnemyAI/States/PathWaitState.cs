using GameSystem.Character.Enemy.Fsm;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI.States
{
    /// <summary>
    ///     路径等待状态 - 当路径上有爆炸威胁时暂停移动
    /// </summary>
    public class PathWaitState : EnemyAIBaseState
    {
        private readonly float maxWaitTime = 3f; // 最大等待时间
        private float currentWaitTime;
        private bool isWaitingForExplosion;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入路径等待状态");
            Owner.statusLog.Add(EnemyAIStates.PathWait);
            Owner.StatusQueue.Enqueue(EnemyAIStates.PathWait);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            Owner.StopMove();
            currentWaitTime = 0f;
            isWaitingForExplosion = true;
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            currentWaitTime += elapseSeconds;

            // 1. 检测爆炸威胁：仍处于爆炸范围内则继续等待
            if (Owner.IsInExplosionRange(Owner.transform.position))
            {
                isWaitingForExplosion = true;
                currentWaitTime = 0f;
                return;
            }

            // 2. 检测玩家（在检查超时前，玩家出现优先级更高）
            var nearestPlayer = Owner.GetNearestPlayer();
            if (nearestPlayer != null)
            {
                var distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                if (distance <= Owner.chaseRange)
                {
                    ChangeState<ChasePlayerState>(fsm);
                    return;
                }
            }

            // 3. 检查等待超时
            if (currentWaitTime >= maxWaitTime)
            {
                // 超时，返回之前的状态
                ReturnToPreviousState(fsm);
                return;
            }

            // 4. 检查路径是否安全：当前位置已不在爆炸范围，且周围有安全出口
            if (!IsPathBlockedByExplosion())
            {
                // 路径安全，返回之前的状态
                ReturnToPreviousState(fsm);
                return;
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开路径等待状态");
        }

        /// <summary>
        ///     返回之前的状态
        /// </summary>
        private void ReturnToPreviousState(IFsm<EnemyAIController> fsm)
        {
            // TODO: 根据上下文决定返回SearchState还是ChasePlayerState
            var states = Owner.StatusQueue.Peek();
            switch (states)
            {
                case EnemyAIStates.Search:
                    ChangeState<SearchState>(fsm);
                    break;
                case EnemyAIStates.ChasePlayer:
                    ChangeState<ChasePlayerState>(fsm);
                    break;
                default:
                    ChangeState<SearchState>(fsm);
                    break;
            }
        }

        /// <summary>
        ///     检查路径是否被爆炸阻挡
        ///     检查四个方向：如果所有安全出口都在爆炸范围内，则路径被阻挡
        /// </summary>
        private bool IsPathBlockedByExplosion()
        {
            var currentPos = Owner.ToBombPutPos(Owner.transform.position);
            var directions = new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (var dir in directions)
            {
                var neighborPos = currentPos + dir;
                // 如果邻居可行走且不在爆炸范围内，说明有安全出口
                if (Owner.MapInfo.IsWalkable(neighborPos) && !Owner.IsInExplosionRange(neighborPos))
                    return false;
            }

            // 所有方向都被爆炸阻挡或不可行走
            return true;
        }
    }
}