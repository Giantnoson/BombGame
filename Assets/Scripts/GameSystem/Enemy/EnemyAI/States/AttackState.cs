
using UnityEngine;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 攻击状态 - 对玩家进行攻击
    /// </summary>
/// <summary>
/// 攻击状态类，继承自EnemyAIBaseState，用于控制敌人的攻击行为
/// </summary>
    public class AttackState : EnemyAIBaseState
    {
        private Transform targetPlayer; // 目标玩家的Transform对象
        private float attackInterval = 1f; // 攻击间隔时间，单位为秒
        private float lastAttackTime; // 上次攻击的时间戳

        /// <summary>
        /// 进入攻击状态时调用的方法
        /// </summary>
        /// <param name="fsm">有限状态机接口</param>
        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入攻击状态");
            Owner.StatusLog.Add(EnemyAIStates.Attack);
            Owner.StatusQueue.Enqueue(EnemyAIStates.Attack);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            Owner.StopMove(); // 停止移动
            targetPlayer = Owner.GetNearestPlayer(); // 获取最近的玩家目标
            lastAttackTime = Time.time - attackInterval; // 允许立即攻击
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds, float realElapseSeconds)
        {
            // 检查状态
            CheckState(fsm);

            // 尝试攻击
            if (Time.time - lastAttackTime >= attackInterval)
            {
                TryAttack();
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            //Owner.isMoving = false;
            Owner.StopMove();
            Debug.Log("离开攻击状态");
        }

        /// <summary>
        /// 检查当前状态
        /// </summary>
        private void CheckState(IFsm<EnemyAIController> fsm)
        {
            // 先扫描周围区域，确保地图数据是最新的
            Owner.mapScan.ScanArea(Owner.transform.position, Mathf.CeilToInt(Owner.detectionRange));

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
            float distance = Vector3.Distance(Owner.transform.position, targetPlayer.position);
            if (distance > Owner.attackRange)
            {
                // 玩家超出攻击范围，切换到追击状态
                ChangeState<ChasePlayerState>(fsm);
                return;
            }
        }

        /// <summary>
        /// 尝试攻击
        /// </summary>
        private void TryAttack()
        {
            if (targetPlayer == null) return;

            // 检查是否可以放置炸弹
            if (Owner.currentBombCount > 0 && Owner.currentBombCooldown <= 0)
            {
                // 放置炸弹攻击
                Owner.PlaceBomb();
                lastAttackTime = Time.time;
            }
            else
            {
                // 无法放置炸弹，切换到追击状态
                ChangeState<ChasePlayerState>(fsm as IFsm<EnemyAIController>);
            }
        }
    }
}
