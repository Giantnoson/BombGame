using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Enemy
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
        ///     是否已放置炸弹
        /// </summary>
        private bool bombPlaced;

        private Vector3 bombPosition;

        /// <summary>
        ///     当前延迟
        /// </summary>
        private float currentDelay;


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
            currentDelay = 0f;
        }


        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            if (!bombPlaced)
            {
                currentDelay += elapseSeconds;
                if (currentDelay >= placeDelay) PlaceBomb();
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