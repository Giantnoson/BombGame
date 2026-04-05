using GameSystem.Character.Enemy.Fsm;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI.States
{
    /// <summary>
    ///     攻击状态 - 对玩家进行攻击
    /// </summary>
    /// <summary>
    ///     攻击状态类，继承自EnemyAIBaseState，用于控制敌人的攻击行为
    /// </summary>
    public class AttackState : EnemyAIBaseState
    {
        private readonly float attackInterval = 1f; // 攻击间隔时间，单位为秒
        private bool isAttackSuccess; // 是否成功攻击
        private float lastAttackTime; // 上次攻击的时间戳
        private Transform targetPlayer; // 目标玩家的Transform对象

        /// <summary>
        ///     进入攻击状态时调用的方法
        /// </summary>
        /// <param name="fsm">有限状态机接口</param>
        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入攻击状态");
            Owner.statusLog.Add(EnemyAIStates.Attack);
            Owner.StatusQueue.Enqueue(EnemyAIStates.Attack);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            Owner.StopMove(); // 停止移动
            targetPlayer = Owner.GetNearestPlayer(); // 获取最近的玩家目标
            lastAttackTime = Time.time - attackInterval; // 允许立即攻击
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            // 检查状态
            CheckState(fsm);

            // 尝试攻击
            if (Time.time - lastAttackTime >= attackInterval) TryAttack();
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            //Owner.isMoving = false;
            Owner.StopMove();
            Debug.Log("离开攻击状态");
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
                targetPlayer = Owner.GetNearestPlayer();
                if (targetPlayer == null)
                {
                    // 没有玩家，切换到搜索状态
                    ChangeState<SearchState>(fsm);
                    return;
                }
            }

            // 3. 检查玩家是否在攻击范围内
            var distance = Vector3.Distance(Owner.transform.position, targetPlayer.position);
            if (distance > Owner.attackRange)
                // 玩家超出攻击范围，切换到追击状态
                ChangeState<ChasePlayerState>(fsm);
        }

        /// <summary>
        ///     尝试攻击
        /// </summary>
        private void TryAttack()
        {
            if (targetPlayer == null) return;

            // 检查是否可以放置炸弹
            if (Owner.bombCooldown <= 0 && Owner.bombCount > 0)
            {
                // 放置炸弹攻击
                Debug.Log("尝试放置炸弹进行攻击");
                Owner.PutBomb(x => { isAttackSuccess = x; });
                lastAttackTime = Time.time;
            }
            else
            {
                // 无法放置炸弹，可能是因为冷却或数量不足，先尝试追击保持距离
                ChangeState<ChasePlayerState>(fsm);
            }
        }
    }
}