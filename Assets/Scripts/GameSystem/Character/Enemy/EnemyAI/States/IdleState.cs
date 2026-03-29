using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.Map;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Enemy
{
    /// <summary>
    ///     闲置状态 - 扫描周围环境
    /// </summary>
    public class IdleState : EnemyAIBaseState
    {
        private readonly float scanInterval = 0.5f; // 扫描间隔
        private bool hasDestructibleBlock;
        private bool hasTarget;
        private float lastScanTime;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入闲置状态");
            Owner.StatusQueue.Enqueue(EnemyAIStates.Idle);
            Owner.statusLog.Add(EnemyAIStates.Idle);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            hasDestructibleBlock = false;
            hasTarget = true;
            Owner.StopMove();
            lastScanTime = Time.time;
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds,
            float realElapseSeconds)
        {
            // 定期扫描环境
            if (Time.time - lastScanTime >= scanInterval)
            {
                lastScanTime = Time.time;
                ScanEnvironment();
            }
        }

        protected internal override void OnLeave(IFsm<EnemyAIController> fsm, bool isShutdown)
        {
            Debug.Log("离开闲置状态");
        }

        /// <summary>
        /// 扫描周围环境
        /// </summary>
        private void ScanEnvironment()
        {
            // 1. 优先检测爆炸威胁
            if (Owner.IsInExplosionRange(Owner.transform.position))
            {
                ChangeState<AvoidExplosionState>(fsm);
                return;
            }

            // 2. 检测是否有玩家
            var nearestPlayer = Owner.GetNearestPlayer();
            if (nearestPlayer != null)
            {
                var distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                if (distance <= Owner.chaseRange)
                {
                    var searchTag = Owner.MapInfo.SearchTag(nearestPlayer.position, TagType.Player);
                    if (searchTag != null)
                    {
                        ChangeState<ChasePlayerState>(fsm);
                        return;
                    }
                }
            }
            
            if (!hasTarget)
            {
            }

            // 3. 检测是否有可破坏的方块
            if (HasDestructibleInRange())
            {
                ChangeState<SearchState>(fsm);
                return;
            }

            if (HasDestructible()) ChangeState<SearchState>(fsm);
        }

        /// <summary>
        ///     检测范围内是否有可破坏的方块
        /// </summary>
        private bool HasDestructibleInRange()
        {
            var targetStep = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible,
                Mathf.CeilToInt(Owner.detectionRange));
            if (targetStep != null)
            {
                Debug.Log("存在可破坏的方块");
                hasDestructibleBlock = true;
                foreach (var stepTracker in targetStep)
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                    {
                        Debug.Log("存在不在爆炸范围的可破坏方块");
                        return true;
                    }

                return false;
            }

            Debug.Log("此范围内不存在可破坏的方块");
            hasDestructibleBlock = false;
            return false;
        }

        private bool HasDestructible()
        {
            var target = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible);
            if (target != null)
            {
                var ishasDestructibleBlockInExplosionRange = false;
                Debug.Log("存在可破坏的方块");
                hasDestructibleBlock = true;
                foreach (var stepTracker in target)
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                    {
                        Debug.Log("存在不在爆炸范围的可破坏方块");
                        ishasDestructibleBlockInExplosionRange = true;
                        break;
                    }

                if (ishasDestructibleBlockInExplosionRange)
                {
                    List<TargetStepInfo> safeBlocks = new List<TargetStepInfo>();
                    foreach (var stepTracker in target)
                    {
                        if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                        {
                            safeBlocks.Add(stepTracker);
                        }
                    }

                    if (safeBlocks.Count > 0)
                    {
                        ChangeState<SearchState>(fsm);
                        return true;
                    }
                }

                Debug.Log("全局不存在不在爆炸范围的可破坏方块");
                return false;
            }

            Debug.Log("全局不存在不在爆炸范围的可破坏方块,选择查找玩家");
            if (FindPlayer())
            {
                ChangeState<ChasePlayerState>(fsm);
                return true;
            }
            Debug.Log("全局不存在不在爆炸范围的可破坏方块,采取随机移动策略");
            hasDestructibleBlock = false;
            MoveInRand();
            ChangeState<SearchState>(fsm);
            hasTarget = false;
            return false;
        }

        private bool FindPlayer()
        {
            var target = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Player);
            if (target != null)
            {
                var hasPlayerInExportRange = false;
                Debug.Log("存在玩家");
                hasDestructibleBlock = true;
                foreach (var stepTracker in target)
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                    {
                        Debug.Log("存在不在爆炸范围的玩家");
                        hasPlayerInExportRange = true;
                        break;
                    }

                if (hasPlayerInExportRange)
                {
                    return true;
                }

                Debug.Log("全局不存在不在爆炸范围的玩家");
                return false;
            }

            return false;
        }

        private void MoveInRand()
        {
            var pointInArea = Owner.MapInfo.GetRandomPointInArea(Owner.ToBombPutPos(Owner.transform.position),
                Mathf.CeilToInt(Owner.detectionRange));
            if (pointInArea != null)
                Owner.MoveTo(Owner.MapInfo.SearchPath(Owner.transform.position,
                    Owner.MapInfo.GetRealCoord(pointInArea.Pos), true));
        }
    }
}