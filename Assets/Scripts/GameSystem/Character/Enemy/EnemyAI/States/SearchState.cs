using System.Collections.Generic;
using Config;
using GameSystem.Character.Enemy.Fsm;
using GameSystem.Map;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI.States
{
    /// <summary>
    ///     搜索状态类，继承自EnemyAIBaseState，用于控制敌人的搜索行为
    /// </summary>
    public class SearchState : EnemyAIBaseState
    {
        private readonly float maxWaitTime = 3f;

        // 目标检查间隔时间（秒）
        private readonly float targetCheckInterval = 0.2f;

        private Vector3 beforePosition;

        // 是否有目标
        private bool hasTarget;

        // 上次检查时间
        private float lastCheckTime;

        private float lastCheckWaitTime;

        // 目标位置
        private Vector3 targetPosition;

        /// <summary>
        ///     进入状态时的回调函数
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入搜索状态");
            Owner.statusLog.Add(EnemyAIStates.Search);
            Owner.StatusQueue.Enqueue(EnemyAIStates.Search);
            Owner.StatusQueue.Dequeue();
            hasTarget = false;
            Owner.isMoving = false;
            FindNewTarget();
        }

        /// <summary>
        ///     状态更新时的回调函数
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        /// <param name="elapseSeconds">距离上一帧的时间（秒）</param>
        /// <param name="realElapseSeconds">距离状态进入的时间（秒）</param>
        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            // 定期检查状态
            if (Time.time - lastCheckTime >= targetCheckInterval)
            {
                lastCheckTime = Time.time;
                CheckState(fsm);
            }

            if (Time.time - lastCheckWaitTime >= maxWaitTime)
            {
                if (beforePosition == Owner.transform.position)
                {
                    Debug.Log("等待时间过长，重新寻找目标");
                    hasTarget = false;
                    Owner.isMoving = false;
                    FindNewTarget();
                }
                else
                {
                    beforePosition = Owner.transform.position;
                }

                lastCheckWaitTime = Time.time;
            }


            // 移动向目标
            if (hasTarget && !Owner.isMoving) MoveToTarget();
        }

        /// <summary>
        ///     离开状态时的回调函数
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        /// <param name="isShutdown">是否是关闭状态机</param>
        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开搜索状态");
        }

        /// <summary>
        ///     检查当前状态，决定是否需要切换到其他状态
        /// </summary>
        /// <param name="fsm">有限状态机引用</param>
        private void CheckState(IFsm<EnemyAIController> fsm)
        {
            // 1. 检测爆炸威胁（最高优先级）
            if (Owner.IsInExplosionRange(Owner.transform.position))
            {
                ChangeState<AvoidExplosionState>(fsm);
                return;
            }

            // 2. 检测玩家
            var nearestPlayer = Owner.GetNearestPlayer();
            if (nearestPlayer != null)
            {
                var distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                if (distance <= Owner.chaseRange)
                {
                    var pointStepTracker =
                        Owner.MapInfo.SearchPath(Owner.transform.position, nearestPlayer.position, true);
                    if (pointStepTracker != null)
                    {
                        ChangeState<ChasePlayerState>(fsm);
                        return;
                    }
                }
            }

            // 3. 检查目标是否仍然有效（地图同步：目标方块可能已被其他爆炸摧毁）
            if (hasTarget && !Owner.MapInfo.HasTag(targetPosition, TagType.Destructible))
            {
                Debug.Log("目标方块已被摧毁，重新搜索");
                hasTarget = false;
                Owner.isMoving = false;
            }

            // 4. 检查是否到达目标
            if (hasTarget && Vector3.Distance(Owner.transform.position, targetPosition) < Owner.stoppingDistance)
            {
                ChangeState<PlaceBombState>(fsm);
                return;
            }

            // 5. 检查路径是否被爆炸阻挡
            if (IsPathBlockedByExplosion())
            {
                ChangeState<PathWaitState>(fsm);
                return;
            }

            // 6. 没有目标时寻找新目标
            if (!hasTarget) FindNewTarget();
        }

        /// <summary>
        ///     寻找新目标（仅设置targetPosition和hasTarget，不发起移动）
        /// </summary>
        private void FindNewTarget()
        {
            Owner.isMoving = false;
            
            // 先进行范围搜索
            var target = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible,
                Mathf.CeilToInt(Owner.detectionRange));
            if (target != null)
            {
                Debug.Log("范围内存在可破坏的方块");
                foreach (var stepTracker in target)
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                    {
                        targetPosition = Owner.MapInfo.GetRealCoord(stepTracker.Pos);
                        hasTarget = true;
                        return;
                    }
            }

            Debug.Log("范围内找不到可破坏的方块，进行全局扫描");
            FindDestructibleGlobal();
        }


        /// <summary>
        ///     全局扫描可破坏方块（仅设置targetPosition和hasTarget，不发起移动）
        /// </summary>
        private void FindDestructibleGlobal()
        {
            var target = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible);
            if (target != null)
            {
                Debug.Log("全局存在可破坏的方块");
                // 收集所有不在爆炸范围内的安全方块
                var safeBlocks = new List<TargetStepInfo>();
                foreach (var stepTracker in target)
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                        safeBlocks.Add(stepTracker);

                if (safeBlocks.Count > 0)
                {
                    var randomIndex = Random.Range(0, safeBlocks.Count);
                    targetPosition = Owner.MapInfo.GetRealCoord(safeBlocks[randomIndex].Pos);
                    hasTarget = true;
                    return;
                }
            }

            // 没有安全方块，采用随机移动策略
            Debug.Log("全局不存在安全可破坏方块，采取随机移动策略");
            var pointInArea = Owner.MapInfo.GetRandomPointInArea(Owner.ToBombPutPos(Owner.transform.position),
                Mathf.CeilToInt(Owner.detectionRange));
            if (pointInArea != null)
            {
                targetPosition = Owner.MapInfo.GetRealCoord(pointInArea.Pos);
                hasTarget = true;
            }
        }


        /// <summary>
        ///     移动向目标
        /// </summary>
        private void MoveToTarget()
        {
            Debug.Log("移动到目标：" + targetPosition + " vPos:" + Owner.MapInfo.GetVirtualCoord(targetPosition));
            Owner.isMoving = Owner.MoveTo(Owner.MapInfo.SearchPath(Owner.transform.position, targetPosition, true));
            if (!Owner.isMoving) hasTarget = false;
        }

        /// <summary>
        ///     检查路径是否被爆炸阻挡
        /// </summary>
        private bool IsPathBlockedByExplosion()
        {
            if (Owner.IsInExplosionRange(Owner.transform.position)) return true;
            return false;
        }
    }
}