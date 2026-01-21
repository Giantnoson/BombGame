using GameSystem.Pool;
using UnityEngine;

namespace GameSystem.Map
{
    public class TargetStepInfoPool : DataObjectPool<TargetStepInfo>
    {
        public static TargetStepInfoPool Instance { get; private set; }
        
        protected override void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public TargetStepInfo Get(Vector2Int pos, int step)
        {
            var obj =  base.Get();
            obj.Pos = pos;
            obj.Step = step;
            return obj;
        }
    }
}