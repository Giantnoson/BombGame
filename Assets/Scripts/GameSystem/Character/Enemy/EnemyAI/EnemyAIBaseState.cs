using UnityEngine;

namespace GameSystem.Character.Enemy
{
    /// <summary>
    /// 敌人AI状态基类
    /// </summary>
    public abstract class EnemyAIBaseState : FsmState<EnemyAIController>
    {
        protected IFsm<EnemyAIController> fsm;
        
        protected EnemyAIController Owner => fsm?.Owner;

        protected internal override void OnInit(IFsm<EnemyAIController> fsm)
        {
            this.fsm = fsm;
        }

        /// <summary>
        /// 检查当前位置是否在爆炸范围内
        /// </summary>
        protected bool IsInExplosionRange(Vector3 position) => Owner.IsInExplosionRange(position);

        /// <summary>
        /// 计算到目标位置的路径
        /// </summary>
        protected Vector3[] CalculatePath(Vector3 targetPosition)
        {
            // 简单实现：返回直接路径
            // TODO: 可以使用A*或NavMesh实现更复杂的路径计算
            return new Vector3[] { targetPosition };
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        protected void MoveToPosition(Vector3 position)
        {
            if (Owner != null)
            {
                Owner.MoveTo(position);
            }
        }
    }
}