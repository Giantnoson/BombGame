
using UnityEngine;

namespace GameSystem.Character.Enemy
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
            //Owner.isMoving = false;
            Owner.StopMove();
            Owner.StatusLog.Add(EnemyAIStates.AvoidExplosion);
            Owner.StatusQueue.Enqueue(EnemyAIStates.AvoidExplosion);
            Owner.StatusQueue.Dequeue();
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
                Debug.Log("确认是否安全");
                lastCheckTime = Time.time;
                CheckSafety(fsm);
            }

            // 移动向安全位置
            if (hasSafePosition && !Owner.isMoving)
            {
                Debug.Log("存在安全的地方，移动到安全的地点:" + safePosition);
                MoveToSafePosition();
            }
            else if(!hasSafePosition)
            {
                Debug.Log("没有安全的地方，随机移动");
                var pointInArea =Owner.MapInfo.GetRandomPointInArea(Owner.ToBombPutPos(Owner.transform.position),
                    Mathf.CeilToInt(Owner.detectionRange));
                if (pointInArea != null)
                {
                    hasSafePosition = true;
                    safePosition = Owner.MapInfo.GetRealCoord(pointInArea.Pos);
                    
                }
                else
                {
                    Debug.Log("没有找到安全的地方，随机移动失败");
                }
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开躲避爆炸状态");
            //Owner.isMoving = false;
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
                Debug.Log("当前位置安全");
                if (wasChasingPlayer)
                {
                    // 之前在追击玩家，检查玩家是否还在追击范围内
                    Transform nearestPlayer = Owner.GetNearestPlayer();
                    if (nearestPlayer != null)
                    {
                        float distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                        if (distance <= Owner.chaseRange)
                        {
                            var pointStepTracker = Owner.MapInfo.SearchPath(Owner.transform.position, nearestPlayer.position);
                            if (pointStepTracker != null)
                            {
                                ChangeState<ChasePlayerState>(fsm);
                                return;
                            }
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
                Debug.Log("当前位置不安全");
                // 安全位置不再安全，重新寻找
                Owner.isMoving = false;
                //hasSafePosition = false;//下面方法已经设置了
                FindSafePosition();
            }
        }

        /// <summary>
        /// 寻找安全位置
        /// </summary>
        private void FindSafePosition()
        {
            Vector3 safePosition = Owner.findSafePosition(Owner.transform.position);
            if (safePosition == Vector3.down)
            {
                // 没有找到安全位置
                hasSafePosition = false;
            }
            else
            {
                // 找到安全位置
                this.safePosition = safePosition;
                hasSafePosition = true;
            }
        }

        /// <summary>
        /// 移动向安全位置
        /// </summary>
        private void MoveToSafePosition()
        {
            if (hasSafePosition && !Owner.isMoving)
                Owner.MoveTo(safePosition);
            //Owner.isMoving = true;
        }
    }
}
