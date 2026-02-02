using System;
using System.Collections.Generic;
using Config;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameSystem.Map
{
    /// <summary>
    ///     地图扫描，要求地图关于（0，0）对称
    ///     请将该组件放置到起始点位置
    /// </summary>
    public class MapScan : MonoBehaviour
    {
        [Header("地图扫描设置")] [Tooltip("扫描起始参照点")]
        public float startY = 0.5f; // 扫描的起始Y坐标

        [Tooltip("偏移偏移距离（包含（0.5,0.5），到边界距离)")] public int offsetDistance = 15; // 扫描偏移距离，决定扫描范围

        public List<TagPointPairs> TagPointPairsList = new(); // 标签点对列表

        public MapScanConfig ScanConfig; // 地图扫描配置

        [Header("局部扫描调试")] [Tooltip("局部扫描的中心位置")]
        public Vector3 referencePos = new(0.5f, 0.5f, 0.5f); // 局部扫描的中心位置

        [Tooltip("局部扫描的半径")] public int referenceOffset = 5; // 局部扫描的半径

        [Header("打印调试")] public bool isPrint = true; // 是否打印调试信息


        //用于存放碰撞器
        private readonly Collider[] _hitColliders = new Collider[20];
        public Dictionary<string, char> TagPointPairsMap = new(); // 标签点对映射字典
        public MapData MapData { get; private set; } // 地图数据属性

        /// <summary>
        ///     唤醒方法，用于初始化配置
        /// </summary>
        private void Awake()
        {
            ScanConfig = Resources.Load<MapScanConfig>(gameObject.scene.name + "/MapScanConfig");
            if (ScanConfig == null)
            {
                Debug.LogError($"在{gameObject.scene.name}/MapScanConfig下未找到MapScanConfig");
            }
            else
            {
                TagPointPairsList = ScanConfig.tagPointPairsList;
                offsetDistance = ScanConfig.offsetDistance;
                startY = ScanConfig.startY;
            }
        }

        private void Start()
        {
            // 初始化地图数据
            MapData = new MapData(offsetDistance, TagPointPairsList);
            foreach (var tagPointPairs in TagPointPairsList)
                if (TagPointPairsMap.ContainsKey(tagPointPairs.tag.ToString()))
                    Debug.LogError("重复的Tag");
                else
                    TagPointPairsMap.Add(tagPointPairs.tag.ToString(), tagPointPairs.point);

            // 扫描地图
            ScanAllMap();
            MapData.PrintMap();
        }

        private void Update()
        {
            if (!isPrint)
            {
                ScanArea(referencePos, referenceOffset);
                MapData.PrintMap();
                isPrint = true;
            }
        }


        public bool IsValidPosition(Vector3 pos)
        {
            return IsValidPosition(GetVirtualCoord(pos));
        }

        /// <summary>
        ///     判断点是否合法
        /// </summary>
        /// <param name="pos">判断的点</param>
        /// <returns>返回是否合法</returns>
        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < MapData.Map.GetLength(0) && pos.y >= 0 && pos.y < MapData.Map.GetLength(1);
        }

        #region 扫描地图

        /// <summary>
        ///     扫描地图，将地图数据存入MapData.Map中
        /// </summary>
        public void ScanAllMap()
        {
            var pos = new Vector3(0, startY, 0);
            for (var j = 0; j < offsetDistance * 2; j++)
            for (var i = 0; i < offsetDistance * 2; i++)
            {
                // print(i + "," + j);
                pos.x = i - offsetDistance + 0.5f;
                pos.z = j - offsetDistance + 0.5f;
                ScanCollider(pos, i, j);
            }
        }

        public void ScanArea(Vector3 refPos, int refOffset)
        {
            var virtualCoord = GetVirtualCoord(refPos);
            ScanArea(virtualCoord, refOffset);
        }


        public void ScanArea(Vector2Int refPos, int refOffset)
        {
            var startX = refPos.x - refOffset + 1;
            var endX = refPos.x + refOffset - 1;
            var startZ = refPos.y - refOffset + 1;
            var endZ = refPos.y + refOffset - 1;

            print("扫描中心坐标:" + refPos.x + "," + refPos.y + "\n扫描范围:x = " + startX + "," + endX + ", z = " + startZ +
                  "," + endZ);
            var pos = new Vector3(0, startY, 0);
            for (var j = Mathf.Min(Mathf.Max(startZ, 0), offsetDistance * 2 - 1);
                 j < offsetDistance * 2 && j <= endZ;
                 j++)
            for (var i = Mathf.Min(Mathf.Max(startX, 0), offsetDistance * 2 - 1);
                 i < offsetDistance * 2 && i <= endX;
                 i++)
            {
                pos.x = i - offsetDistance + 0.5f;
                pos.z = j - offsetDistance + 0.5f;
                ScanCollider(pos, i, j);
            }
        }

        private void ScanCollider(Vector3 pos, int i, int j)
        {
            var flag = false;
            var colliderCount = Physics.OverlapBoxNonAlloc(pos, new Vector3(0.4f, 0.4f, 0.4f), _hitColliders);
            for (var k = 0; k < colliderCount; k++)
                if (TagPointPairsMap.ContainsKey(_hitColliders[k].tag))
                {
                    if (flag)
                    {
                        if (_hitColliders[k].CompareTag(nameof(ObjectType.Bomb))) //如果已经存在一个标签，则判断是否为炸弹，是则优先赋炸弹
                            MapData.Map[i, j] = TagPointPairsMap[_hitColliders[k].tag]; //赋值
                    }
                    else
                    {
                        flag = true;
                        MapData.Map[i, j] = TagPointPairsMap[_hitColliders[k].tag]; //赋值
                    }
                }

            if (!flag) MapData.Map[i, j] = TagPointPairsMap[nameof(ObjectType.Nothing)]; //当无标签时
        }


        public void ScanPoint(Vector3 pos)
        {
            pos.y = startY;
            var point = GetVirtualCoord(pos);
            var flag = false;
            var colliderCount = Physics.OverlapBoxNonAlloc(pos, new Vector3(0.4f, 0.4f, 0.4f), _hitColliders);
            for (var k = 0; k < colliderCount; k++)
                if (TagPointPairsMap.ContainsKey(_hitColliders[k].tag))
                {
                    if (flag)
                    {
                        if (_hitColliders[k].CompareTag(nameof(ObjectType.Bomb))) //如果已经存在一个标签，则判断是否为炸弹，是则优先赋炸弹
                            MapData.Map[point.x, point.y] = TagPointPairsMap[_hitColliders[k].tag]; //赋值
                    }
                    else
                    {
                        flag = true;
                        MapData.Map[point.x, point.y] = TagPointPairsMap[_hitColliders[k].tag]; //赋值
                    }
                }

            if (!flag) MapData.Map[point.x, point.y] = TagPointPairsMap[nameof(ObjectType.Nothing)]; //当无标签时
        }

        public void ScanPoint(Vector2Int pos)
        {
            var v3Pso = GetRealCoord(pos);
            var flag = false;
            var colliderCount = Physics.OverlapBoxNonAlloc(v3Pso, new Vector3(0.4f, 0.4f, 0.4f), _hitColliders);
            for (var k = 0; k < colliderCount; k++)
                if (TagPointPairsMap.ContainsKey(_hitColliders[k].tag))
                {
                    if (flag)
                    {
                        if (_hitColliders[k].CompareTag(nameof(ObjectType.Bomb))) //如果已经存在一个标签，则判断是否为炸弹，是则优先赋炸弹
                            MapData.Map[pos.x, pos.y] = TagPointPairsMap[_hitColliders[k].tag]; //赋值
                    }
                    else
                    {
                        flag = true;
                        MapData.Map[pos.x, pos.y] = TagPointPairsMap[_hitColliders[k].tag]; //赋值
                    }
                }

            if (!flag) MapData.Map[pos.x, pos.y] = TagPointPairsMap[nameof(ObjectType.Nothing)]; //当无标签时
        }

        #endregion


        #region 坐标转换

        /// <summary>
        ///     获取虚拟坐标
        /// </summary>
        /// <param name="pos"> 真实坐标 </param>
        /// <returns>返回虚拟坐标</returns>
        public Vector2Int GetVirtualCoord(Vector3 pos)
        {
            return new Vector2Int(Mathf.FloorToInt(pos.x) + offsetDistance, Mathf.FloorToInt(pos.z) + offsetDistance);
        }

        /// <summary>
        ///     获取真实坐标
        /// </summary>
        /// <param name="virtualCoord"> 虚拟坐标 </param>
        /// <returns>返回真实坐标</returns>
        public Vector3 GetRealCoord(Vector2Int virtualCoord)
        {
            return new Vector3(virtualCoord.x - offsetDistance + 0.5f, startY, virtualCoord.y - offsetDistance + 0.5f);
        }

        #endregion


        #region 坐标tag比较

        /// <summary>
        ///     判断是否可行走
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <returns>返回是否可行走</returns>
        public bool IsWalkable(Vector3 pos)
        {
            var virtualCoord = GetVirtualCoord(pos);
            return IsWalkable(virtualCoord);
        }

        /// <summary>
        ///     判断是否可行走
        /// </summary>
        /// <param name="virtualPos">虚拟坐标</param>
        /// <returns>返回是否可行走</returns>
        public bool IsWalkable(Vector2Int virtualPos)
        {
            return MapData.Map[virtualPos.x, virtualPos.y] == TagPointPairsMap[nameof(ObjectType.Nothing)];
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <param name="type">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector3 pos, ObjectType type)
        {
            var virtualCoord = GetVirtualCoord(pos);
            return CompareTag(virtualCoord, type);
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag
        /// </summary>
        /// <param name="virtualPos">真实坐标</param>
        /// <param name="type">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector2Int virtualPos, ObjectType type)
        {
            return MapData.Map[virtualPos.x, virtualPos.y] == TagPointPairsMap[type.ToString()];
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的坐标列表中的一个tag
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTags(Vector3 pos, ObjectType[] types)
        {
            var virtualCoord = GetVirtualCoord(pos);
            return CompareTags(virtualCoord, types);
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag列表中的一个tag
        /// </summary>
        /// <param name="virtualPos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag列表中的一个tag</returns>
        public bool CompareTags(Vector2Int virtualPos, ObjectType[] types)
        {
            foreach (var type in types)
                if (MapData.Map[virtualPos.x, virtualPos.y] == TagPointPairsMap[type.ToString()])
                    return true;
            return false;
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的坐标列表中的一个tag
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTags(Vector3 pos, List<ObjectType> types)
        {
            var virtualCoord = GetVirtualCoord(pos);
            return CompareTags(virtualCoord, types);
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag列表中的一个tag
        /// </summary>
        /// <param name="virtualPos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag列表中的一个tag</returns>
        public bool CompareTags(Vector2Int virtualPos, List<ObjectType> types)
        {
            foreach (var type in types)
                if (MapData.Map[virtualPos.x, virtualPos.y] == TagPointPairsMap[type.ToString()])
                    return true;
            return false;
        }

        #endregion


        #region 判断两个点是否可达

        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空
        ///     注意：使用前请更新地图
        /// </summary>
        /// <param name="startPos">开始坐标</param>
        /// <param name="endPos">结束坐标</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> ExistPath(Vector3 startPos, Vector3 endPos, int maxArea)
        {
            if (IsValidPosition(startPos) && IsValidPosition(endPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                var endVirtualCoord = GetVirtualCoord(endPos);
                return ExistPath(startVirtualCoord, endVirtualCoord, maxArea);
            }

            Debug.LogError("不合法的位置: " + startPos + " or " + endPos);
            return null;
        }


        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空
        ///     注意：请更新地图，且更新区域为以角色为中心，范围为maxArea
        /// </summary>
        /// <param name="startVirtualPos">开始坐标</param>
        /// <param name="endVirtualPos">结束坐标</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> ExistPath(Vector2Int startVirtualPos, Vector2Int endVirtualPos, int maxArea)
        {
            if (!(IsValidPosition(startVirtualPos) && IsValidPosition(endVirtualPos)))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos + " or " + endVirtualPos);
                return null;
            }

            //结果数组
            var result = new List<PointStepTracker>();
            //BFS队列
            var que = new Queue<Vector2Int>();
            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var visited = new HashSet<Vector2Int>();

            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);

            var count = 1;
            for (var i = 0; i < maxArea; i++) //最大范围控制
            {
                var bfsCount = count;
                count = 0;
                for (var j = 0; j < bfsCount; j++) //单次
                {
                    if (que.Count == 0) return null;
                    var current = que.Dequeue();
                    if (MapData.Map[current.x, current.y] == TagPointPairsMap[tag])
                        result.Add(new PointStepTracker(current, count));

                    foreach (var direction in directions)
                    {
                        var next = current + direction;
                        if (!IsValidPosition(next)) continue;
                        if (!visited.Contains(next) && IsWalkable(next))
                            if (next == endVirtualPos)
                            {
                                {
                                    result.Add(new PointStepTracker(next, count + 1));
                                }
                                que.Enqueue(next);
                                count++;
                            }

                        visited.Add(next);
                    }
                }
            }

            return result.Count == 0 ? null : result;
        }

        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空,此方法采用BFS算法
        ///     注意，请更新整个地图！
        /// </summary>
        /// <param name="startPos">开始坐标</param>
        /// <param name="endPos">结束坐标</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker ExistPath(Vector3 startPos, Vector3 endPos)
        {
            if (IsValidPosition(startPos) && IsValidPosition(endPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                var endVirtualCoord = GetVirtualCoord(endPos);
                return ExistPath(startVirtualCoord, endVirtualCoord);
            }

            Debug.LogError("不合法的位置: " + startPos + " or " + endPos);
            return null;
        }

        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空
        /// </summary>
        /// <param name="startVirtualPos">开始坐标</param>
        /// <param name="endVirtualPos">结束坐标</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker ExistPath(Vector2Int startVirtualPos, Vector2Int endVirtualPos)
        {
            if (!(IsValidPosition(startVirtualPos) && IsValidPosition(endVirtualPos)))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos + " or " + endVirtualPos);
                return null;
            }

            //BFS队列
            var que = new Queue<Vector2Int>();
            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var visited = new HashSet<Vector2Int>();

            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);

            var count = 0;
            while (que.Count > 0)
            {
                var current = que.Dequeue();
                foreach (var direction in directions)
                {
                    var next = current + direction;
                    if (!IsValidPosition(next)) continue;
                    if (!visited.Contains(next) && IsWalkable(next))
                    {
                        if (next == endVirtualPos) return new PointStepTracker(next, count + 1);
                        que.Enqueue(next);
                    }

                    visited.Add(next);
                }

                count++;
            }

            return null;
        }

        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空
        ///     注意：使用前请更新地图
        /// </summary>
        /// <param name="startPos">开始坐标</param>
        /// <param name="endPos">结束坐标</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> ExistPathForFindPlayer(Vector3 startPos, Vector3 endPos, int maxArea)
        {
            if (IsValidPosition(startPos) && IsValidPosition(endPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                var endVirtualCoord = GetVirtualCoord(endPos);
                return ExistPathForFindPlayer(startVirtualCoord, endVirtualCoord, maxArea);
            }

            Debug.LogError("不合法的位置: " + startPos + " or " + endPos);
            return null;
        }


        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空
        ///     注意：请更新地图，且更新区域为以角色为中心，范围为maxArea
        /// </summary>
        /// <param name="startVirtualPos">开始坐标</param>
        /// <param name="endVirtualPos">结束坐标</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> ExistPathForFindPlayer(Vector2Int startVirtualPos, Vector2Int endVirtualPos,
            int maxArea)
        {
            if (!(IsValidPosition(startVirtualPos) && IsValidPosition(endVirtualPos)))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos + " or " + endVirtualPos);
                return null;
            }

            //结果数组
            var result = new List<PointStepTracker>();
            //BFS队列
            var que = new Queue<Vector2Int>();
            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var visited = new HashSet<Vector2Int>();

            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);

            var count = 1;
            for (var i = 0; i < maxArea; i++) //最大范围控制
            {
                var bfsCount = count;
                count = 0;
                for (var j = 0; j < bfsCount; j++) //单次
                {
                    if (que.Count == 0) return null;
                    var current = que.Dequeue();
                    if (MapData.Map[current.x, current.y] == TagPointPairsMap[tag])
                        result.Add(new PointStepTracker(current, count));

                    foreach (var direction in directions)
                    {
                        var next = current + direction;
                        if (!IsValidPosition(next)) continue;
                        if (!visited.Contains(next) && (IsWalkable(next) || CompareTag(next, ObjectType.Player)))
                            if (next == endVirtualPos)
                            {
                                {
                                    result.Add(new PointStepTracker(next, count + 1));
                                }
                                que.Enqueue(next);
                                count++;
                            }

                        visited.Add(next);
                    }
                }
            }

            return result.Count == 0 ? null : result;
        }

        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空,此方法采用BFS算法
        ///     注意，请更新整个地图！
        /// </summary>
        /// <param name="startPos">开始坐标</param>
        /// <param name="endPos">结束坐标</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker ExistPathForFindPlayer(Vector3 startPos, Vector3 endPos)
        {
            if (IsValidPosition(startPos) && IsValidPosition(endPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                var endVirtualCoord = GetVirtualCoord(endPos);
                return ExistPathForFindPlayer(startVirtualCoord, endVirtualCoord);
            }

            Debug.LogError("不合法的位置: " + startPos + " or " + endPos);
            return null;
        }

        /// <summary>
        ///     判断两个点是否可达,若不可达则返回空
        /// </summary>
        /// <param name="startVirtualPos">开始坐标</param>
        /// <param name="endVirtualPos">结束坐标</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker ExistPathForFindPlayer(Vector2Int startVirtualPos, Vector2Int endVirtualPos)
        {
            if (!(IsValidPosition(startVirtualPos) && IsValidPosition(endVirtualPos)))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos + " or " + endVirtualPos);
                return null;
            }

            //BFS队列
            var que = new Queue<Vector2Int>();
            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var visited = new HashSet<Vector2Int>();

            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);

            var count = 0;
            while (que.Count > 0)
            {
                var current = que.Dequeue();
                foreach (var direction in directions)
                {
                    var next = current + direction;
                    if (!IsValidPosition(next)) continue;
                    if (!visited.Contains(next) && (IsWalkable(next) || CompareTag(next, ObjectType.Player)))
                    {
                        if (next == endVirtualPos) return new PointStepTracker(next, count + 1);
                        que.Enqueue(next);
                    }

                    visited.Add(next);
                }

                count++;
            }

            return null;
        }

        #endregion

        #region 搜索在限定深度/广度下目标tag的可达并附加距离

        /// <summary>
        ///     搜索在限定深度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxStep">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> SearchTagWithDFS(Vector3 startPos, ObjectType tag, int maxStep)
        {
            if (IsValidPosition(startPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                return SearchTagWithDFS(startVirtualCoord, tag, maxStep);
            }

            Debug.LogError("不合法的位置: " + startPos);
            return null;
        }

        /// <summary>
        ///     搜索在限定深度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startVirtualPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxStep">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> SearchTagWithDFS(Vector2Int startVirtualPos, ObjectType tag, int maxStep)
        {
            if (!IsValidPosition(startVirtualPos))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos);
                return null;
            }

            var result = new List<PointStepTracker>();
            var stack = new Stack<Vector2Int>();
            var visited = new Dictionary<Vector2Int, int>();
            var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            stack.Push(startVirtualPos);
            visited.Add(startVirtualPos, 0);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited[current] >= maxStep)
                    continue;
                foreach (var direction in directions)
                {
                    var next = current + direction;
                    if (!IsValidPosition(next)) continue;
                    if (CompareTag(next, tag) && !visited.ContainsKey(next)) //当标签正确时返回前一步的结果
                        result.Add(new PointStepTracker(current, visited[current]));

                    if (IsWalkable(next) && !CompareTag(next, ObjectType.Wall))
                    {
                        if (visited.ContainsKey(next)) //当下个地方坐标正确且可行走时，如果找到此处有个key
                        {
                            if (visited[next] > visited[current] + 1) //且这个key的步数大于现在的步数
                                visited[next] = visited[current] + 1;
                            else
                                continue;
                        }
                        else
                        {
                            visited.Add(next, visited[current] + 1);
                        }

                        stack.Push(next);
                    }
                }
            }

            return result.Count == 0 ? null : result;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker SearchTagWithBFSForOne(Vector3 startPos, ObjectType tag)
        {
            if (IsValidPosition(startPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                return SearchTagWithBFSForOne(startVirtualCoord, tag);
            }

            Debug.LogError("不合法的位置: " + startPos);
            return null;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startVirtualPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker SearchTagWithBFSForOne(Vector2Int startVirtualPos, ObjectType tag)
        {
            if (!IsValidPosition(startVirtualPos))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos);
                return null;
            }

            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };


            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);
            var count = 0;
            while (que.Count > 0)
            {
                count++;
                var current = que.Dequeue();

                foreach (var vector2Int in directions)
                {
                    var next = current + vector2Int;
                    if (!IsValidPosition(next)) continue;
                    if (MapData.Map[current.x, current.y] == TagPointPairsMap[tag.ToString()])
                        return new PointStepTracker(current, count);

                    if (!visited.Contains(next) && IsWalkable(next))
                    {
                        que.Enqueue(next);
                        visited.Add(next);
                    }
                }
            }

            return null;
        }


        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker SearchTagWithBFSForOne(Vector3 startPos, ObjectType tag, int maxArea)
        {
            if (IsValidPosition(startPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                return SearchTagWithBFSForOne(startVirtualCoord, tag, maxArea);
            }

            Debug.LogError("不合法的位置: " + startPos);
            return null;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startVirtualPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public PointStepTracker SearchTagWithBFSForOne(Vector2Int startVirtualPos, ObjectType tag, int maxArea)
        {
            if (!IsValidPosition(startVirtualPos))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos);
                return null;
            }

            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };


            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);
            var count = 1;
            var bfsCount = 1;
            for (var i = 0; i < maxArea; i++) //最大范围控制
            {
                bfsCount = count;
                count = 0;
                for (var j = 0; j < bfsCount; j++) //单次
                {
                    if (que.Count == 0) return null;
                    var current = que.Dequeue();

                    foreach (var vector2Int in directions)
                    {
                        var next = current + vector2Int;
                        if (!IsValidPosition(next)) continue;
                        if (MapData.Map[next.x, next.y] == TagPointPairsMap[tag.ToString()])
                            return new PointStepTracker(current, count + 1);
                        if (!visited.Contains(next) && IsWalkable(next))
                        {
                            que.Enqueue(next);
                            visited.Add(next);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> SearchTagWithBFS(Vector3 startPos, ObjectType tag, int maxArea)
        {
            if (IsValidPosition(startPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                return SearchTagWithBFS(startVirtualCoord, tag, maxArea);
            }

            Debug.LogError("不合法的位置: " + startPos);
            return null;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startVirtualPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> SearchTagWithBFS(Vector2Int startVirtualPos, ObjectType tag, int maxArea)
        {
            if (!IsValidPosition(startVirtualPos))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos);
                return null;
            }

            var result = new List<PointStepTracker>();
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };


            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);
            var count = 1;
            var bfsCount = 1;
            for (var i = 0; i < maxArea; i++) //最大范围控制
            {
                bfsCount = count;
                count = 0;
                for (var j = 0; j < bfsCount; j++) //单次
                {
                    if (que.Count == 0) return null;
                    var current = que.Dequeue();

                    foreach (var vector2Int in directions)
                    {
                        var next = current + vector2Int;
                        if (!IsValidPosition(next)) continue;
                        if (MapData.Map[next.x, next.y] == TagPointPairsMap[tag.ToString()])
                        {
                            result.Add(new PointStepTracker(current, count + 1));
                            visited.Add(next);
                            continue;
                        }

                        if (!visited.Contains(next) && IsWalkable(next))
                        {
                            que.Enqueue(next);
                            visited.Add(next);
                            count++;
                        }
                    }
                }
            }

            return result.Count == 0 ? null : result;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> SearchTagWithBFS(Vector3 startPos, ObjectType tag)
        {
            if (IsValidPosition(startPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                return SearchTagWithBFS(startVirtualCoord, tag);
            }

            Debug.LogError("不合法的位置: " + startPos);
            return null;
        }

        /// <summary>
        ///     搜索在限定广度下目标tag的可达并附加距离
        ///     注意，请更新地图
        /// </summary>
        /// <param name="startVirtualPos">起始位置</param>
        /// <param name="tag">目标对象</param>
        /// <param name="maxArea">最大步长</param>
        /// <returns>返回起始位置到布标位置包含步长的列表</returns>
        public List<PointStepTracker> SearchTagWithBFS(Vector2Int startVirtualPos, ObjectType tag)
        {
            if (!IsValidPosition(startVirtualPos))
            {
                Debug.LogError("不合法的位置: " + startVirtualPos);
                return null;
            }

            var result = new List<PointStepTracker>();
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };


            que.Enqueue(startVirtualPos);
            visited.Add(startVirtualPos);
            var count = 1;
            while (que.Count > 0)
            {
                var current = que.Dequeue();
                foreach (var vector2Int in directions)
                {
                    var next = current + vector2Int;
                    if (!IsValidPosition(next)) continue;
                    if (MapData.Map[next.x, next.y] == TagPointPairsMap[tag.ToString()])
                    {
                        result.Add(new PointStepTracker(current, count + 1));
                        visited.Add(next);
                        continue;
                    }

                    if (!visited.Contains(next) && IsWalkable(next))
                    {
                        que.Enqueue(next);
                        visited.Add(next);
                        count++;
                    }
                }
            }

            return result.Count == 0 ? null : result;
        }

        #endregion

        #region 获取随机位置

        public PointStepTracker GetRandomPointInArea(Vector3 startPos, int area)
        {
            if (IsValidPosition(startPos))
            {
                var startVirtualCoord = GetVirtualCoord(startPos);
                return GetRandomPointInArea(startVirtualCoord, area);
            }

            Debug.LogError("不合法的位置: " + startPos);
            return null;
        }


        public PointStepTracker GetRandomPointInArea(Vector2Int startPos, int area)
        {
            if (!IsValidPosition(startPos))
            {
                Debug.LogError("不合法的位置: " + startPos);
                return null;
            }

            var tryCount = offsetDistance * offsetDistance;
            var minx = Mathf.CeilToInt(MathF.Min(Mathf.Max(startPos.x - area + 1, 0), offsetDistance * 2));
            var maxx = Mathf.CeilToInt(MathF.Min(Mathf.Min(startPos.x + area - 1, offsetDistance * 2),
                MapData.Map.GetLength(0)));
            var miny = Mathf.CeilToInt(MathF.Min(Mathf.Max(startPos.y - area + 1, 0), offsetDistance * 2));
            var maxy = Mathf.CeilToInt(MathF.Min(Mathf.Min(startPos.y + area - 1, offsetDistance * 2),
                MapData.Map.GetLength(1)));
            var point = new Vector2Int();
            while (tryCount > 0)
            {
                point.x = Random.Range(minx, maxx);
                point.y = Random.Range(miny, maxy);
                if (IsWalkable(point)) return new PointStepTracker(point, 0);
                tryCount--;
            }

            return null;
        }

        #endregion
    }


    public class PointStepTracker
    {
        public Vector2Int Point;
        public int Step;

        public PointStepTracker()
        {
        }

        public PointStepTracker(Vector2Int point, int step)
        {
            Point = point;
            Step = step;
        }
    }
}