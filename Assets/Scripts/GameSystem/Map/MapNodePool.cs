using GameSystem.Pool;
using UnityEngine;

namespace GameSystem.Map
{
    /// <summary>
    ///     MapNode对象池，用于复用MapNode对象，避免频繁创建和销毁
    /// </summary>
    public class MapNodePool : DataObjectPool<MapNode>
    {
        public static MapNodePool Instance { get; private set; }

        protected override void InitializeSingleton()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public MapNode Get(Vector2Int pos, int step)
        {
            var obj = Get();
            obj.CurrentPos = pos;
            return obj;
        }

        public new MapNode Get()
        {
            var obj = base.Get();
            obj.NeighborNodes.Clear();
            obj.CurrentTagOb.Clear();
            return obj;
        }
    }
}