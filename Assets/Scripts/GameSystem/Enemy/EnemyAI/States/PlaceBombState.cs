
using UnityEngine;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 放置炸弹状态 - 在合适位置放置炸弹
    /// </summary>
    public class PlaceBombState : EnemyAIBaseState
    {
        private bool bombPlaced;
        private float placeDelay = 0.5f;
        private float currentDelay;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入放置炸弹状态");
            Owner.StopMove();
            bombPlaced = false;
            currentDelay = 0f;
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            if (!bombPlaced)
            {
                currentDelay += elapseSeconds;
                if (currentDelay >= placeDelay)
                {
                    PlaceBomb();
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
            Debug.Log("离开放置炸弹状态");
        }

        /// <summary>
        /// 放置炸弹
        /// </summary>
        private void PlaceBomb()
        {
            if (Owner.currentBombCount > 0 && Owner.currentBombCooldown <= 0)
            {
                Owner.PlaceBomb();
                bombPlaced = true;
            }
            else
            {
                // 无法放置炸弹，切换到闲置状态
                ChangeState<IdleState>(fsm as IFsm<EnemyAIController>);
            }
        }
    }
}
