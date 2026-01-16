
using UnityEngine;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 躲避爆炸状态 - 躲避爆炸威胁，寻找安全区域
    /// </summary>
    public class AvoidExplosionState : EnemyAIBaseState
    {
        private Vector3 safePosition;
        private bool hasSafePosition;
        private float safetyCheckInterval = 0.2f;
        private float lastCheckTime;
        private bool wasChasingPlayer;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入躲避爆炸状态");
            hasSafePosition = false;
            FindSafePosition();

            // 记录是否在追击玩家
            Transform nearestPlayer = Owner.GetNearestPlayer();
            wasChasingPlayer = (nearestPlayer != null && 
                               Vector3.Distance(Owner.transform.position, nearestPlayer.position) <= Owner.chaseRange);
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            // 定期检查安全状态
            if (Time.time - lastCheckTime >= safetyCheckInterval)
            {
                lastCheckTime = Time.time;
                CheckSafety(fsm);
            }

            // 移动向安全位置
            if (hasSafePosition)
            {
                MoveToSafePosition();
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开躲避爆炸状态");
            Owner.StopMove();
        }

        /// <summary>
        /// 检查安全状态
        /// </summary>
        private void CheckSafety(IFsm<EnemyAIController> fsm)
        {
            // 1. 检查当前位置是否安全
            if (!Owner.IsInExplosionRange(Owner.transform.position))
            {
                // 当前位置安全
                if (wasChasingPlayer)
                {
                    // 之前在追击玩家，检查玩家是否还在追击范围内
                    Transform nearestPlayer = Owner.GetNearestPlayer();
                    if (nearestPlayer != null)
                    {
                        float distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                        if (distance <= Owner.chaseRange)
                        {
                            // 玩家仍在追击范围内，继续追击
                            ChangeState<ChasePlayerState>(fsm);
                            return;
                        }
                    }
                }

                // 切换到闲置状态
                ChangeState<IdleState>(fsm);
                return;
            }

            // 2. 检查安全位置是否仍然安全
            if (hasSafePosition && Owner.IsInExplosionRange(safePosition))
            {
                // 安全位置不再安全，重新寻找
                FindSafePosition();
            }
        }

        /// <summary>
        /// 寻找安全位置
        /// </summary>
        private void FindSafePosition()
        {
            // TODO: 实现更智能的安全位置查找算法
            // 临时：在当前位置周围随机查找安全位置
            Vector3[] possiblePositions = new Vector3[8];
            possiblePositions[0] = Owner.transform.position + Vector3.forward * 2f;
            possiblePositions[1] = Owner.transform.position + Vector3.back * 2f;
            possiblePositions[2] = Owner.transform.position + Vector3.left * 2f;
            possiblePositions[3] = Owner.transform.position + Vector3.right * 2f;
            possiblePositions[4] = Owner.transform.position + new Vector3(2f, 0, 2f);
            possiblePositions[5] = Owner.transform.position + new Vector3(-2f, 0, 2f);
            possiblePositions[6] = Owner.transform.position + new Vector3(2f, 0, -2f);
            possiblePositions[7] = Owner.transform.position + new Vector3(-2f, 0, -2f);

            foreach (var pos in possiblePositions)
            {
                if (!Owner.IsInExplosionRange(pos))
                {
                    safePosition = pos;
                    safePosition.y = Owner.transform.position.y;
                    hasSafePosition = true;
                    return;
                }
            }

            // 没有找到安全位置
            hasSafePosition = false;
        }

        /// <summary>
        /// 移动向安全位置
        /// </summary>
        private void MoveToSafePosition()
        {
            if (!hasSafePosition) return;

            Owner.MoveTo(safePosition);
        }
    }
}
