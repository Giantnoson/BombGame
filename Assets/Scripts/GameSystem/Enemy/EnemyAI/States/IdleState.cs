
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Enemy
{
    /// <summary>
    /// 闲置状态 - 扫描周围环境
    /// </summary>
    public class IdleState : EnemyAIBaseState
    {
        private float scanInterval = 0.5f; // 扫描间隔
        private float lastScanTime;

        protected internal override void OnEnter(IFsm<EnemyAIController> fsm)
        {
            Debug.Log("进入闲置状态");
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
                    ChangeState<ChasePlayerState>(fsm);
                    return;
                }
            }

            // 3. 检测是否有可破坏的方块
            if (HasDestructibleInRange())
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
            Vector2Int enemyVirtualCoord = Owner.mapScan.GetVirtualCoord(Owner.transform.position);
            char destructible = Owner.mapScan.TagPointPairsMap[ObjectType.Destructible.ToString()];
            Owner.mapScan.ScanArea(enemyVirtualCoord, Mathf.CeilToInt(Owner.detectionRange));//对地图进行扫描
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            
            
            
            
            
            
            
            
            return false;
        }
        
        
        
        
        
        
        
    }
}
