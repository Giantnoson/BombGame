using GameSystem.Character.Enemy.Fsm;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI.States
{
    /// <summary>
    ///     放置炸弹状态 - 在合适位置放置炸弹
    /// </summary>
    /// <summary>
    ///     放置炸弹状态类，继承自EnemyAIBaseState
    /// </summary>
    public class PlaceBombState : EnemyAIBaseState
    {
        /// <summary>
        ///     放置延迟
        /// </summary>
        private readonly float placeDelay = 0.5f;

        /// <summary>
        ///     最大等待回调超时时间（秒），防止回调丢失导致状态卡死
        /// </summary>
        private readonly float maxCallbackTimeout = 2f;

        /// <summary>
        ///     是否已放置炸弹
        /// </summary>
        private bool bombPlaced;

        /// <summary>
        ///     是否已发起放置请求
        /// </summary>
        private bool hasRequestedPlace;

        private Vector3 bombPosition;

        /// <summary>
        ///     当前延迟
        /// </summary>
        private float currentDelay;

        /// <summary>
        ///     回调等待累计时间
        /// </summary>
        private float callbackWaitTime;


        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入放置炸弹状态");
            Owner.statusLog.Add(EnemyAIStates.PlaceBomb);
            Owner.StatusQueue.Enqueue(EnemyAIStates.PlaceBomb);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            Owner.StopMove();
            // bombPlaced = Owner.transform.position == bombPosition;
            bombPlaced = false;
            hasRequestedPlace = false;
            currentDelay = 0f;
            callbackWaitTime = 0f;
        }


        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            if (!bombPlaced)
            {
                currentDelay += elapseSeconds;
                if (currentDelay >= placeDelay && !hasRequestedPlace) 
                    PlaceBomb();
                
                // 超时保护：如果已发起请求但回调一直未触发，超时后切换状态
                if (hasRequestedPlace)
                {
                    callbackWaitTime += elapseSeconds;
                    if (callbackWaitTime >= maxCallbackTimeout)
                    {
                        Debug.LogWarning("放置炸弹回调超时，切换到闲置状态");
                        ChangeState<IdleState>(fsm);
                    }
                }
            }
            else
            {
                // 炸弹已放置，立即切换到躲避状态
                ChangeState<AvoidExplosionState>(fsm);
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            //Owner.isMoving = false;//无移动
            Debug.Log("离开放置炸弹状态");
        }

        /// <summary>
        ///     放置炸弹
        /// </summary>
        private void PlaceBomb()
        {
            if (Owner.bombCount > 0 && Owner.bombCooldown <= 0)
            {
                Debug.Log("放置炸弹");
                hasRequestedPlace = true;
                Owner.PutBomb(x => { bombPlaced = x; });
                bombPosition = Owner.transform.position;
            }
            else
            {
                // 无法放置炸弹，切换到闲置状态
                ChangeState<IdleState>(fsm);
            }
        }
    }
}