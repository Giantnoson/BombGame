using System;
using System.Collections.Generic;
using System.Linq;
using config;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameSystem.Map
{
    public class MapInfo : MonoBehaviour
    {
        [Header("地图扫描设置")]
        [Tooltip("扫描起始参照点")]
        public float startY = 0.5f;                    // 扫描的起始Y坐标
        [Tooltip("偏移偏移距离（包含（0.5,0.5），到边界距离)")]
        public int offsetDistance = 15;                // 扫描偏移距离，决定扫描范围
        [Tooltip("地图数据")]
        public Dictionary<Vector2Int, MapNode> _mapData = new Dictionary<Vector2Int, MapNode>();

        public Dictionary<Vector2Int, MapNode> MapData
        {
            get => _mapData;
            private set => _mapData = value;
        }         // 地图数据属性

        private List<TagType> TagList = new List<TagType>();
        private Dictionary<string,TagType> TagMap = new Dictionary<string,TagType>();

        public MapInfoConfig InfoConfig;              // 地图扫描配置

        [Header("局部扫描调试")]
        [Tooltip("局部扫描的中心位置")]
        public Vector3 referencePos = new Vector3(0.5f,0.5f,0.5f);  // 局部扫描的中心位置
        [Tooltip("局部扫描的半径")]
        public int referenceOffset = 5;               // 局部扫描的半径

        [Header("调试信息")]
        [Tooltip("打印调试")]
        public bool isPrint = true;                   // 是否打印调试信息


        [Tooltip("调试起点")]
        [SerializeField]
        public Vector2Int TStart;
        [Tooltip("调试终点")]
        [SerializeField]
        public Vector2Int TEnd;
        
        
        //用于存放碰撞器
        private Collider[] _hitColliders = new Collider[20];

        // 对象池引用
        private MapNodePool _nodePool;
        
        private Vector2Int[] _direction = new []{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};

        private Vector3[] _v3direction = new []{Vector3.forward,Vector3.back,Vector3.left,Vector3.right};


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                PrintMap();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ScanAllMap();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                var info = SearchPath(TStart, TEnd);
                Debug.Log(info);
            }
        }

        private void PrintMap()
        {
            foreach (var mapNode in MapData)
            {
                if (mapNode.Value != null)
                {
                    Debug.Log(mapNode.Key + " : " + mapNode.Value);
                }
            }
        }


        private void Awake()
        {
            // 获取或创建对象池
            _nodePool = FindObjectOfType<MapNodePool>();
            if (_nodePool == null)
            {
                GameObject poolObj = new GameObject("MapNodePool");
                _nodePool = poolObj.AddComponent<MapNodePool>();
            }

            InfoConfig = Resources.Load<MapInfoConfig>(gameObject.scene.name + "/MapInfoConfig");
            if (InfoConfig == null)
            {
                Debug.LogError("未找到MapInfoConfig");
            }
            else
            {
                TagList = InfoConfig.tagList;
                offsetDistance = InfoConfig.offsetDistance;
                startY = InfoConfig.startY;
            }
        }

        private void Start()
        {
            //初始化TagMap
            foreach (var tags in TagList)
            {
                if (TagMap.ContainsKey(tags.ToString()))
                {
                    Debug.LogError("TagMap中已存在该标签");
                }
                else
                {
                    TagMap.Add(tags.ToString(),tags);
                }
            }
            //初始化MapData
            MapData = new Dictionary<Vector2Int, MapNode>();
            ScanAllMap();
        }

        #region 基础扫图

        public void ScanAllMap()
        {
            Vector3 pos = new Vector3(0 , startY, 0);
            for (int j = 0; j < offsetDistance * 2; j++)
            {
                for (int i = 0; i < offsetDistance * 2; i++)
                {
                    // print(i + "," + j);
                    pos.x = i - offsetDistance + 0.5f;
                    pos.z = j - offsetDistance + 0.5f;
                    ScanCollider(pos, i, j);
                }
            }
            InitNeighbor();
        }

        private void ScanCollider(Vector3 v3Pos, int i, int j)
        {
            Vector2Int key = new Vector2Int(i,j);
            if (MapData.ContainsKey(key))
            {
                return;
            }
            var node = _nodePool.Get();
            node.CurrentPos = key;
            bool flag = false;
            int colliderCount = Physics.OverlapBoxNonAlloc(v3Pos, new Vector3(0.4f, 0.4f, 0.4f),_hitColliders);
            for (int k = 0; k < colliderCount; k++)
            {
                if (_hitColliders[k].CompareTag(nameof(TagType.Wall)))
                {
                    _nodePool.Return(node);
                    return;
                }
                if (TagMap.ContainsKey(_hitColliders[k].tag))
                {
                    node.CurrentTag.Add(TagMap[_hitColliders[k].tag]);//添加标签
                    flag = true;
                }
            }
            if (!flag)
            {
                node.CurrentTag.Add(TagType.Nothing);//添加标签
            }
            MapData.Add(key, node);//添加节点
        }

        private void InitNeighbor()
        {
            foreach (KeyValuePair<Vector2Int, MapNode> node in _mapData)
            {
                foreach (Vector2Int dir in _direction)
                {
                    Vector2Int keyDir = node.Key + dir;
                    if (MapData.TryGetValue(keyDir, out var neighborNode))
                    {
                        node.Value.AddNeighbor(neighborNode);
                    }
                }
            }
        }
        
        
        /// <summary>
        /// 更新所有地图数据
        /// </summary>
        public void UpdateMapForAll()
        {
            foreach (KeyValuePair<Vector2Int, MapNode> node in _mapData)
            {
                ScanPoint(node.Key, node.Value);
            }
        }

        /// <summary>
        /// 扫描指定位置，重用传入的节点对象
        /// </summary>
        /// <param name="v3Pos">扫描位置</param>
        /// <param name="node">要重用的节点对象</param>
        public void ScanPoint(Vector3 v3Pos, MapNode node)
        {
            v3Pos.y = startY;
            bool flag = false;
            // 清空现有标签
            node.CurrentTag.Clear();

            int colliderCount = Physics.OverlapBoxNonAlloc(v3Pos, new Vector3(0.4f, 0.4f, 0.4f),_hitColliders);
            for (int k = 0; k < colliderCount; k++)
            {
                if (TagMap.ContainsKey(_hitColliders[k].tag))
                {
                    node.CurrentTag.Add(TagMap[_hitColliders[k].tag]);//添加标签
                    flag = true;
                }
            }
            if (!flag)
            {
                node.CurrentTag.Add(TagType.Nothing);//添加标签
            }
        }

        /// <summary>
        /// 扫描指定位置，重用传入的节点对象
        /// </summary>
        /// <param name="pos">扫描位置</param>
        /// <param name="node">要重用的节点对象</param>
        public void ScanPoint(Vector2Int pos, MapNode node)
        {
            Vector3 v3Pso = GetRealCoord(pos);
            bool flag = false;
            node.CurrentTag.Clear();

            int colliderCount = Physics.OverlapBoxNonAlloc(v3Pso, new Vector3(0.4f, 0.4f, 0.4f),_hitColliders);
            for (int i = 0; i < colliderCount; i++)
            {
                if (TagMap.ContainsKey(_hitColliders[i].tag))
                {
                    if(_hitColliders[i].CompareTag(nameof(TagType.Player)) || _hitColliders[i].CompareTag(nameof(TagType.Enemy)))
                    {
                        if (GetVirtualCoord(_hitColliders[i].transform.position) != pos)
                        {
                            continue;
                        }
                    }
                    node.CurrentTag.Add(TagMap[_hitColliders[i].tag]);
                    flag = true;
                }
            }

            if (!flag)
            {
                node.CurrentTag.Add(TagType.Nothing);//添加标签
            }
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 获取虚拟坐标
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <returns>虚拟坐标</returns>
        public Vector2Int GetVirtualCoord(Vector3 pos)
        {
            return new Vector2Int(Mathf.FloorToInt(pos.x) + offsetDistance, Mathf.FloorToInt(pos.z) + offsetDistance);
        }

        /// <summary>
        /// 获取真实坐标
        /// </summary>
        /// <param name="virtualCoord">虚拟坐标</param>
        /// <returns>真实坐标</returns>
        public Vector3 GetRealCoord(Vector2Int virtualCoord)
        {
            return new Vector3(virtualCoord.x - offsetDistance + 0.5f, startY, virtualCoord.y - offsetDistance + 0.5f);
        }

        #endregion

        #region 区域搜索
        
        /// <summary>
        /// 判断是否可行走
        /// </summary>
        /// <param name="v3Pos">真实坐标</param>
        /// <returns>返回是否可行走</returns>
        public bool IsWalkable(Vector3 v3Pos)
        {
            return CompareTag(v3Pos, TagType.Nothing);
        }

        /// <summary>
        /// 判断是否可行走
        /// </summary>
        /// <param name="pos">虚拟坐标</param>
        /// <returns>返回是否可行走</returns>
        public bool IsWalkable(Vector2Int pos)
        {
            return CompareTag(pos, TagType.Nothing);
        }
        /// <summary>
        /// 比较该坐标下的点是否是所需的tag
        /// </summary>
        /// <param name="v3Pos">真实坐标</param>
        /// <param name="type">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector3 v3Pos, TagType type)
        {
            Vector2Int virtualCoord = GetVirtualCoord(v3Pos);
            return CompareTag(virtualCoord, type);
        }

        /// <summary>
        /// 比较该坐标下的点是否是所需的tag
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <param name="type">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector2Int pos, TagType type)
        {
            if (!_mapData.ContainsKey(pos))
            {
                return false;
            }
            ScanPoint(pos, MapData[pos]);
            foreach (TagType tagType in MapData[pos].CurrentTag)
            {
                if (tagType == type)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 比较该坐标下的点是否是所需的tag中的一个
        /// </summary>
        /// <param name="v3Pos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector3 v3Pos, List<TagType> types)
        {
            Vector2Int virtualCoord = GetVirtualCoord(v3Pos);
            return CompareTag(virtualCoord, types);
        }

        /// <summary>
        /// 比较该坐标下的点是否是所需的tag中的一个
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector2Int pos, List<TagType> types)
        {
            if (!_mapData.ContainsKey(pos))
            {
                Debug.LogError($"坐标{pos}不存在");
                return false;
            }
            ScanPoint(pos, MapData[pos]);
            foreach (TagType tagType in MapData[pos].CurrentTag)
            {
                if (types.Contains(tagType))
                    return true;
            }
            return false;
        }
        #endregion

        #region 判断路径存在

        //预计使用AStar算法进行实现

        public PathInfo SearchPath(Vector3 v3StartPos, Vector3 v3EndPos)
        {
            return SearchPath(GetVirtualCoord(v3StartPos), GetVirtualCoord(v3EndPos));
        }

        /// <summary>
        /// 使用A*算法搜索路径
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="endPos">目标位置</param>
        /// <returns>路径信息列表，如果找不到路径则返回null</returns>
        public PathInfo SearchPath(Vector2Int startPos, Vector2Int endPos)
        {
            // 检查起点和终点是否在地图数据中
            if (!MapData.ContainsKey(startPos) || !MapData.ContainsKey(endPos))
            {
                Debug.LogError($"起始点{startPos}或终点{endPos}不存在");
                return null;
            }

            // 初始化数据结构
            //开放列表
            var openList = new MinHeap<MapNode>(CompareNodeByF);
            //闭合列表
            var closeList = new HashSet<MapNode>();
            //路径追溯
            var cameFrom = new Dictionary<MapNode, MapNode>(); 
            //从起点到当前节点的实际代价
            var gScore = new Dictionary<MapNode, float>();
            // 获取起点和终点节点
            var startNode = MapData[startPos];
            var endNode = MapData[endPos];
            // 初始化起点的g值和f值
            gScore[startNode] = 0;
            startNode.F = Heuristic(startNode, endNode);
            // 将起点加入开放列表
            openList.Add(startNode);
            while (openList.Count > 0)
            {
                // 获取f值最小的节点
                var currentNode = openList.Pop();


                
                
                // 如果到达目标节点，构建并返回路径
                if (currentNode == endNode)
                    return ReconstructPath(cameFrom, currentNode, startPos, endPos);
                // 将当前节点加入关闭列表
                closeList.Add(currentNode);
                // 遍历当前节点的所有邻居
                foreach (var neighbor in currentNode.MapNodes)
                {
                    // 如果邻居在关闭列表中，跳过
                    if (closeList.Contains(neighbor))
                        continue;
                    //当此点不能行走时，跳过
                    if (!IsWalkable(currentNode.CurrentPos))
                    {
                        closeList.Add(currentNode);
                        continue;
                    }
                    // 计算从起点经过当前节点到邻居的代价
                    var neighborGScore = gScore[currentNode] + DistanceBetween(currentNode, neighbor);
                    // 如果邻居不在开放列表中，或者找到更好的路径
                    if (!openList.Contains(neighbor) || neighborGScore < gScore[neighbor])
                    {
                        // 记录路径
                        cameFrom[neighbor] = currentNode;
                        // 更新g值和f值
                        gScore[neighbor] = neighborGScore;
                        neighbor.F = gScore[neighbor] + Heuristic(neighbor, endNode);
                        // 如果邻居不在开放列表中，加入开放列表
                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
            // 无法找到路径
            return null;
        }

        /// <summary>
        /// 路径回溯，构建完整路径
        /// </summary>
        private PathInfo ReconstructPath(Dictionary<MapNode, MapNode> cameFrom, MapNode current, Vector2Int startPos, Vector2Int endPos)
        {
            var totalPath = new List<MapNode> { current };
            // 从终点回溯到起点
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            // 反转路径，使其从起点到终点
            totalPath.Reverse();
            // 转换为PathInfo列表
            var path = new List<Vector2Int>();
            foreach (var node in totalPath)
                path.Add(node.CurrentPos);
            var targetInfo = new TargetInfo
            {
                Pos = endPos,
                Type = MapData[endPos].CurrentTag.Count > 0 ? MapData[endPos].CurrentTag.First() : TagType.Nothing
            };
            return new PathInfo(startPos, targetInfo, path);
        }

        /// <summary>
        /// 计算两个节点之间的距离（移动代价）
        /// </summary>
        private float DistanceBetween(MapNode a, MapNode b)
        {
            // 在网格地图中，相邻节点之间的距离通常为1
            return Vector2Int.Distance(a.CurrentPos,b.CurrentPos);
        }

        /// <summary>
        /// 启发函数，估计从当前节点到目标节点的代价
        /// </summary>
        private float Heuristic(MapNode a, MapNode b)
        {
            // 使用曼哈顿距离作为启发函数
            return Mathf.Abs(a.CurrentPos.x - b.CurrentPos.x) + Mathf.Abs(a.CurrentPos.y - b.CurrentPos.y);
        }

        /// <summary>
        /// 比较两个节点的f值，用于优先队列排序
        /// </summary>
        private int CompareNodeByF(MapNode x, MapNode y)
        {
            return x.F.CompareTo(y.F);
        }
        

        #endregion


        #region 搜索算法BFS
        public List<TargetStepInfo> SearchTags(Vector3 v3StartPos, TagType tagTypes)
        {
            var _tag = new List<TagType>();
            _tag.Add(tagTypes);
            return SearchTags(GetVirtualCoord(v3StartPos), _tag);
        }
        public List<TargetStepInfo> SearchTags(Vector3 v3StartPos, List<TagType> tagTypes)
        {
            return SearchTags(GetVirtualCoord(v3StartPos), tagTypes);
        }

        
        public List<TargetStepInfo> SearchTags(Vector2Int startPos, List<TagType> tagTypes)
        {
            if (!MapData.ContainsKey(startPos))//不存在位置时，返回空
            {
                Debug.LogWarning("此位置不存在" + startPos);
               return null;
            }
            
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            
            que.Enqueue(startPos);
            visited.Add(startPos);
            var count = 0;
            var result = new List<TargetStepInfo>();


            while (que.Count > 0)
            {
                count++;
                var current = que.Dequeue();
                
                var currentNode = MapData[current];
                foreach (var currentNodeMapNode in currentNode.MapNodes)
                {
                    if(visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                    if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes))
                    {
                        result.Add(TargetStepInfoPool.Instance.Get(currentNodeMapNode.CurrentPos, count));
                        visited.Add(currentNodeMapNode.CurrentPos);
                        continue;
                    }
                    visited.Add(currentNodeMapNode.CurrentPos);
                    que.Enqueue(currentNodeMapNode.CurrentPos);
                }
            }
            return result.Count == 0 ? null : result;
        }

        public List<TargetStepInfo> SearchTags(Vector3 v3StartPos, TagType tagTypes, int maxArea)
        {
            var _tag = new List<TagType>();
            _tag.Add(tagTypes);
            return SearchTags(GetVirtualCoord(v3StartPos), _tag, maxArea);
        }
        
        public List<TargetStepInfo> SearchTags(Vector3 v3StartPos, List<TagType> tagTypes, int maxArea)
        {
            return SearchTags(GetVirtualCoord(v3StartPos), tagTypes, maxArea);
        }

        
        public List<TargetStepInfo> SearchTags(Vector2Int startPos, List<TagType> tagTypes, int maxArea)
        {
            if (!MapData.ContainsKey(startPos))//不存在位置时，返回空
            {
                Debug.LogWarning("此位置不存在" + startPos);
                return null;
            }
            
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            
            que.Enqueue(startPos);
            visited.Add(startPos);
            var count = 1;
            var result = new List<TargetStepInfo>();
            for (int i = 0; i < maxArea; i++)
            {
                var bfsCount = count;
                count = 0;
                for (int j = 0; j < bfsCount; j++)
                {
                    var current = que.Dequeue();
                    var currentNode = MapData[current];
                    foreach (var currentNodeMapNode in currentNode.MapNodes)
                    {
                        if(visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                        if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes))
                        {
                            result.Add(TargetStepInfoPool.Instance.Get(currentNodeMapNode.CurrentPos, i));
                            visited.Add(currentNodeMapNode.CurrentPos);
                            continue;
                        }
                        visited.Add(currentNodeMapNode.CurrentPos);
                        que.Enqueue(currentNodeMapNode.CurrentPos);
                        count++;
                    }
                }
            }
            return result.Count == 0 ? null : result;
        }
        
        public TargetStepInfo SearchTag(Vector3 v3StartPos, TagType tagTypes)
        {
            var _tag = new List<TagType>();
            _tag.Add(tagTypes);
            return SearchTag(GetVirtualCoord(v3StartPos), _tag);
        }
        public TargetStepInfo SearchTag(Vector3 v3StartPos, List<TagType> tagTypes)
        {
            return SearchTag(GetVirtualCoord(v3StartPos), tagTypes);
        }

        public TargetStepInfo SearchTag(Vector2Int startPos, TagType tagTypes)
        {
            var _tag = new List<TagType>();
            _tag.Add(tagTypes);
            return SearchTag(startPos, _tag);
        }
        
        public TargetStepInfo SearchTag(Vector2Int startPos, List<TagType> tagTypes)
        {
            if (!MapData.ContainsKey(startPos))//不存在位置时，返回空
            {
                Debug.LogWarning("此位置不存在" + startPos);
               return null;
            }
            
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            
            que.Enqueue(startPos);
            visited.Add(startPos);
            var count = 0;

            while (que.Count > 0)
            {
                count++;
                var current = que.Dequeue();
                
                var currentNode = MapData[current];
                foreach (var currentNodeMapNode in currentNode.MapNodes)
                {
                    if(visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                    if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes))
                    {
                        return TargetStepInfoPool.Instance.Get(currentNodeMapNode.CurrentPos, count);
                    }
                    visited.Add(currentNodeMapNode.CurrentPos);
                    que.Enqueue(currentNodeMapNode.CurrentPos);
                }
            }
            return null;
        }
        public TargetStepInfo SearchTag(Vector3 v3StartPos, TagType tagTypes, int maxArea)
        {
            var _tag = new List<TagType>();
            _tag.Add(tagTypes);
            return SearchTag(GetVirtualCoord(v3StartPos), _tag, maxArea);
        }
        
        public TargetStepInfo SearchTag(Vector3 v3StartPos, List<TagType> tagTypes, int maxArea)
        {
            return SearchTag(GetVirtualCoord(v3StartPos), tagTypes, maxArea);
        }

        
        public TargetStepInfo SearchTag(Vector2Int startPos, List<TagType> tagTypes, int maxArea)
        {
            if (!MapData.ContainsKey(startPos))//不存在位置时，返回空
            {
                Debug.LogWarning("此位置不存在" + startPos);
                return null;
            }
            
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            
            que.Enqueue(startPos);
            visited.Add(startPos);
            var count = 1;
            for (int i = 0; i < maxArea; i++)
            {
                var bfsCount = count;
                count = 0;
                for (int j = 0; j < bfsCount; j++)
                {
                    var current = que.Dequeue();
                    var currentNode = MapData[current];
                    foreach (var currentNodeMapNode in currentNode.MapNodes)
                    {
                        if(visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                        if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes))
                        { 
                            return TargetStepInfoPool.Instance.Get(currentNodeMapNode.CurrentPos, i);
                        }
                        visited.Add(currentNodeMapNode.CurrentPos);
                        que.Enqueue(currentNodeMapNode.CurrentPos);
                        count++;
                    }
                }
            }

            return null;
        }
        #endregion
        
        
        #region 获取随机位置

        public TargetStepInfo GetRandomPointInArea(Vector3 v3StartPos, int area)
        {
            Vector2Int startPos = GetVirtualCoord(v3StartPos);
            return GetRandomPointInArea(startPos, area);
        }

        
        public TargetStepInfo GetRandomPointInArea(Vector2Int startPos, int area)
        {
            if (!IsValidPosition(startPos))
            {
                Debug.LogError("不合法的位置: " + startPos);
                return null;
            }
            int tryCount = offsetDistance * offsetDistance;
            int minx = Mathf.CeilToInt(MathF.Min(Mathf.Max(startPos.x - area + 1, 0),offsetDistance * 2));
            var maxx = Mathf.CeilToInt(Mathf.Min(startPos.x + area - 1, offsetDistance * 2));
            var miny = Mathf.CeilToInt(MathF.Min(Mathf.Max(startPos.y - area + 1, 0),offsetDistance * 2));
            var maxy = Mathf.CeilToInt(Mathf.Min(startPos.y + area - 1, offsetDistance * 2));
            Vector2Int point = new Vector2Int();
            while (tryCount > 0)
            {
                point.x = Random.Range(minx, maxx);
                point.y = Random.Range(miny, maxy);
                if (IsWalkable(point))
                {
                    return new TargetStepInfo(point,0);
                }
                tryCount--;
            }

            return null;
        }
        #endregion
        
        
        public bool IsValidPosition(Vector3 pos)
        {
            return IsValidPosition(GetVirtualCoord(pos));
        }

        /// <summary>
        /// 判断点是否合法
        /// </summary>
        /// <param name="pos">判断的点</param>
        /// <returns>返回是否合法</returns>
        public bool IsValidPosition(Vector2Int pos)
        {
            return _mapData.ContainsKey(pos);
        }
        
        private void OnDestroy()
        {
            // 当对象销毁时，将所有节点归还到对象池
            if (_nodePool != null)
            {
                foreach (var node in MapData.Values)
                {
                    _nodePool.Return(node);
                }
            }
        }
    }



    
    
    /// <summary>
    /// 路径信息
    /// </summary>
    public class PathInfo
    {
        //基本信息
        public Vector2Int StartPos;
        public TargetInfo TargetInfo;
        public readonly List<Vector2Int> Path;
        
        //移动相关
        /// <summary>
        /// 总步数
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// 当前步数
        /// </summary>
        public int CurrentStep;
        
        
        

        public PathInfo(Vector2Int startPos, [NotNull] TargetInfo targetInfo, [NotNull] List<Vector2Int> path)
        {
            StartPos = startPos;
            TargetInfo = targetInfo ?? throw new ArgumentNullException(nameof(targetInfo));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Count = path.Count;
            CurrentStep = 0;
        }

        public bool  NowPath(out Vector2Int nowPos)
        {
            if (CurrentStep < Count)
            {
                nowPos = Path[CurrentStep];
                return true;
            }
            else
            {
                nowPos = Vector2Int.down;
                return false;
            }
        }
        
        public Vector2Int NowPath()
        {
            if (CurrentStep < Count)
            {
                return Path[CurrentStep];
            }
            else
            {
                return Vector2Int.down;
            }
        }

        public bool Next(out Vector2Int nextPos)
        {
            if (CurrentStep < Count - 1)
            {
                nextPos =  Path[++CurrentStep];
                return true;
            }
            nextPos = Vector2Int.down;
            return false;
        }
        
        public Vector2Int Next()
        {
            if (CurrentStep < Count - 1)
            {
                return Path[++CurrentStep];
            }
            return Vector2Int.down;
        }

        public bool GetNextPaths(int step, out List<Vector2Int> path)
        {
            if (CurrentStep >= Count)
            {
                path = null;
                return false;
            }
            if (CurrentStep + step < Count)
                path = Path.GetRange(CurrentStep, step);
            else
                path = Path.GetRange(CurrentStep, Count - CurrentStep);
            return true;
        }

        public Vector2Int GetEndPos()
        {
            return Path[Count - 1];
        }

        public Vector2Int GetStartPos()
        {
            return Path[0];
        }

        public bool IsEnd()
        {
            return CurrentStep == Count - 1;
        }
        
        public PathInfo()
        {
        }
    }

    /// <summary>
    /// 目标信息
    /// </summary>
    public class TargetInfo
    {
        public Vector2Int Pos;
        public TagType Type;
    }

    public class TargetStepInfo
    {
        public TargetStepInfo()
        {
        }

        public TargetStepInfo(Vector2Int pos, int step)
        {
            Pos = pos;
            Step = step;
        }


        public Vector2Int Pos;
        public int Step;
    }
}