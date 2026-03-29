using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Enemy
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

            // 1. 检测爆炸威胁
            if (Owner.IsInExplosionRange(Owner.transform.position))
            {
                // 仍在爆炸范围内，继续等待
                isWaitingForExplosion = true;
                currentWaitTime = 0f;
                return;
            }

            // 2. 检查等待超时
            if (currentWaitTime >= maxWaitTime)
            {
                // 超时，返回之前的状态
                ReturnToPreviousState(fsm);
                return;
            }

            // 3. 检查路径是否安全
            if (!IsPathBlockedByExplosion())
            {
                // 路径安全，返回之前的状态
                ReturnToPreviousState(fsm);
                return;
            }

            // 4. 检测玩家
            var nearestPlayer = Owner.GetNearestPlayer();
            if (nearestPlayer != null)
            {
                var distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                if (distance <= Owner.chaseRange) ChangeState<ChasePlayerState>(fsm);
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
        /// </summary>
        private bool IsPathBlockedByExplosion()
        {
            // TODO: 实现路径爆炸检测
            return false;
        }
    }
}