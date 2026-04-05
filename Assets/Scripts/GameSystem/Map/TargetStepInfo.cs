using UnityEngine;

namespace GameSystem.Map
{
    public class TargetStepInfo
    {
        public Vector2Int Pos;
        public int Step;

        public TargetStepInfo()
        {
        }

        public TargetStepInfo(Vector2Int pos, int step)
        {
            Pos = pos;
            Step = step;
        }
    }
}