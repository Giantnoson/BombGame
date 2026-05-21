using System.Collections.Generic;
using Config;
using GameSystem.Character.Enemy.Fsm;
using GameSystem.Map;
using UnityEngine;

namespace GameSystem.Character.Enemy.EnemyAI.States
{
    /// <summary>
    ///     闲置状态 - 扫描周围环境
    /// </summary>
    public class IdleState : EnemyAIBaseState
    {
        /// <summary>
        ///     扫描结果枚举，用于ScanEnvironment统一处理状态切换
        /// </summary>
        private enum ScanResult { None, Destructible, Player }
        
        private readonly float scanInterval = 0.5f; // 扫描间隔
        private bool hasDestructibleBlock;
        private float lastScanTime;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入闲置状态");
            Owner.StatusQueue.Enqueue(EnemyAIStates.Idle);
            Owner.statusLog.Add(EnemyAIStates.Idle);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            hasDestructibleBlock = false;
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

            // 3. 检测是否有可破坏的方块
            if (HasDestructibleInRange())
            {
                ChangeState<SearchState>(fsm);
                return;
            }

            // 全局扫描：统一处理状态切换，避免内部ChangeState导致的竞争条件
            var scanResult = HasDestructible();
            switch (scanResult)
            {
                case ScanResult.Player:
                    ChangeState<ChasePlayerState>(fsm);
                    break;
                case ScanResult.Destructible:
                    ChangeState<SearchState>(fsm);
                    break;
                case ScanResult.None:
                    // 没有任何目标，采取随机移动策略
                    hasDestructibleBlock = false;
                    MoveInRand();
                    ChangeState<SearchState>(fsm);
                    break;
            }
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
                {
                    var candidatePos = Owner.MapInfo.GetRealCoord(stepTracker.Pos);
                    if (!IsInExplosionRange(candidatePos) && Owner.MapInfo.IsWalkable(candidatePos))
                    {
                        Debug.Log("存在不在爆炸范围的可破坏方块");
                        return true;
                    }
                }

                return false;
            }

            Debug.Log("此范围内不存在可破坏的方块");
            hasDestructibleBlock = false;
            return false;
        }

        /// <summary>
        ///     全局扫描：只返回扫描结果，不切换状态。由ScanEnvironment统一处理状态切换。
        /// </summary>
        private ScanResult HasDestructible()
        {
            var target = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible);
            if (target != null)
            {
                Debug.Log("存在可破坏的方块");
                hasDestructibleBlock = true;
                foreach (var stepTracker in target)
                {
                    var candidatePos = Owner.MapInfo.GetRealCoord(stepTracker.Pos);
                    if (!IsInExplosionRange(candidatePos) && Owner.MapInfo.IsWalkable(candidatePos))
                    {
                        Debug.Log("存在不在爆炸范围的可破坏方块");
                        return ScanResult.Destructible;
                    }
                }

                Debug.Log("全局不存在不在爆炸范围的可破坏方块");
                return ScanResult.None;
            }

            Debug.Log("全局不存在不在爆炸范围的可破坏方块,选择查找玩家");
            if (FindPlayer())
            {
                return ScanResult.Player;
            }
            Debug.Log("全局不存在不在爆炸范围的可破坏方块,也没有玩家,采取随机移动策略");
            return ScanResult.None;
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
                {
                    var candidatePos = Owner.MapInfo.GetRealCoord(stepTracker.Pos);
                    if (!IsInExplosionRange(candidatePos) && Owner.MapInfo.IsWalkable(candidatePos))
                    {
                        Debug.Log("存在不在爆炸范围的玩家");
                        hasPlayerInExportRange = true;
                        break;
                    }
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