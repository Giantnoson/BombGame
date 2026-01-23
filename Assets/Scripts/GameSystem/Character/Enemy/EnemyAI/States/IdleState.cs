
using System.Collections.Generic;
using config;
using UnityEngine;

namespace GameSystem.Character.Enemy
{
    /// <summary>
    /// 闲置状态 - 扫描周围环境
    /// </summary>
    public class IdleState : EnemyAIBaseState
    {
        private float scanInterval = 0.5f; // 扫描间隔
        private float lastScanTime;
        private bool hasDestructibleBlock;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入闲置状态");
            Owner.StatusQueue.Enqueue(EnemyAIStates.Idle);

            Owner.StatusLog.Add(EnemyAIStates.Idle);
            Owner.StatusQueue.Dequeue();
            //Owner.isMoving = false;
            hasDestructibleBlock = false;
            Owner.StopMove();
            lastScanTime = Time.time;
        }

        protected internal override void OnUpdate(IFsm<EnemyAIController> fsm, float elapseSeconds, float realElapseSeconds)
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
            //Owner.isMoving = false;//由于整个过程都无移动操作故此无效
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
            Transform nearestPlayer = Owner.GetNearestPlayer();
            if (nearestPlayer != null)
            {

                float distance = Vector3.Distance(Owner.transform.position, nearestPlayer.position);
                if (distance <= Owner.chaseRange)
                {
                    var searchTag= Owner.MapInfo.SearchTag(nearestPlayer.position, TagType.Player);
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
            if (HasDestructible())
            {
                
                ChangeState<SearchState>(fsm);
                return;
            }
        }

        /// <summary>
        /// 检测范围内是否有可破坏的方块
        /// </summary>
        private bool HasDestructibleInRange()
        {
            var targetStep = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible, Mathf.CeilToInt(Owner.detectionRange));
            if (targetStep != null)
            {
                Debug.Log("存在可破坏的方块");
                hasDestructibleBlock = true;
                foreach (var stepTracker in targetStep)
                {
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
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

        private bool HasDestructible()
        {
            var target = Owner.MapInfo.SearchTags(Owner.transform.position, TagType.Destructible);
            if (target != null)
            {
                bool ishasDestructibleBlockInExplosionRange = false;
                Debug.Log("存在可破坏的方块");
                hasDestructibleBlock = true;
                foreach (var stepTracker in target)
                {
                    if (!IsInExplosionRange(Owner.MapInfo.GetRealCoord(stepTracker.Pos)))
                    {
                        Debug.Log("存在不在爆炸范围的可破坏方块");
                        ishasDestructibleBlockInExplosionRange = true;
                        break;

                    }
                }

                if (ishasDestructibleBlockInExplosionRange)
                {
                    int randomIndex = Random.Range(0, target.Count);
                    var selectedBlock = target[randomIndex];
                    return Owner.MoveTo(Owner.MapInfo.GetRealCoord(selectedBlock.Pos));
                }
                else
                {
                    Debug.Log("全局不存在不在爆炸范围的可破坏方块");
                    return false;
                }

            }
            Debug.Log("全局不存在不在爆炸范围的可破坏方块,采取随机移动策略");
            //采取随机移动的策略;
            var pointInArea = Owner.MapInfo.GetRandomPointInArea(Owner.ToBombPutPos(Owner.transform.position),
                Mathf.CeilToInt(Owner.detectionRange));
            if (pointInArea != null)
                return Owner.MoveTo(Owner.MapInfo.GetRealCoord(pointInArea.Pos));
            return false;
        }
        
        
        
        
        
    }
}
