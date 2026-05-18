using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Config;
using Core.Net;
using GameSystem.GameProps;
using GameSystem.GameScene;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameSystem.Map
{
    public class MapInfo : MonoBehaviour
    {
        public static MapInfo Instance { get; private set; }
        
        [Header("地图扫描设置")] [Tooltip("扫描起始参照点")]
        public float startY = 0.5f; // 扫描的起始Y坐标

        [Tooltip("偏移偏移距离（包含（0.5,0.5），到边界距离)")] public int offsetDistance = 15; // 扫描偏移距离，决定扫描范围

        public MapScanInfoConfig scanInfoConfig; // 地图扫描配置

        [Header("局部扫描调试")] [Tooltip("局部扫描的中心位置")]
        public Vector3 referencePos = new(0.5f, 0.5f, 0.5f); // 局部扫描的中心位置

        [Tooltip("局部扫描的半径")] public int referenceOffset = 5; // 局部扫描的半径

        [Header("调试信息")] [Tooltip("打印调试")] public bool isPrint = true; // 是否打印调试信息


        [Tooltip("调试起点")] [SerializeField] public Vector2Int TStart;

        [Tooltip("调试终点")] [SerializeField] public Vector2Int TEnd;

        private int _count = 0;
        

        private readonly Vector2Int[] _direction =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };


        //用于存放碰撞器
        private readonly Collider[] _hitColliders = new Collider[20];
        private readonly Dictionary<string, TagType> TagMap = new();

        [Tooltip("地图数据")] public Dictionary<Vector2Int, MapNode> _mapData = new();


        public void MapToJosn()
        {
            // 将地图数据转换为可序列化的格式
            var serializableMapData = new SerializableMapData(_mapData);
    
            // 转换为JSON字符串
            var json = JsonUtility.ToJson(serializableMapData, true);
    
            // 打印JSON字符串
            Debug.Log(json);
    
            // 可选：保存到文件
            string filePath = Path.Combine(Application.persistentDataPath, "mapData.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"地图数据已保存到: {filePath}");
        }

        public void MapToNetwork()
        {
    
            // 创建消息体字典
            var body = new NetDictionary();
    
            // 添加地图数据
            var mapDataDict = new NetDictionary();
            foreach (var kvp in _mapData)
            {
                var nodeData = new NetDictionary();
                nodeData["x"] = kvp.Key.x;
                nodeData["y"] = kvp.Key.y;
        
                // 添加标签
                var tagsList = new NetDictionary();
                foreach (var tag in kvp.Value.CurrentTagOb)
                {
                    var tagData = new NetDictionary();
                    tagData["type"] =tag.Value.ToString();
                    tagData["id"] = tag.Key.Id;
                    tagsList[tag.Key.Id] = tagData;
                }
                nodeData["tags"] = tagsList;
        
                // 添加邻居
                var neighborsList = new NetDictionary();
                int index = 0;
                foreach (var neighbor in kvp.Value.NeighborNodes)
                {
                    var neighborData = new NetDictionary();
                    neighborData["x"] = neighbor.CurrentPos.x;
                    neighborData["y"] = neighbor.CurrentPos.y;
                    neighborsList[index.ToString()] = neighborData;
                    index++;
                }
                nodeData["neighbors"] = neighborsList;
        
                mapDataDict[$"{kvp.Key.x}_{kvp.Key.y}"] = nodeData;
            }
    
            body["mapData"] = mapDataDict;
            
            // 可选：保存到文件
            string filePath = Path.Combine(Application.persistentDataPath, "netMapJson.json");
            File.WriteAllText(filePath, body.ToJsonString());
            Debug.Log($"地图数据已保存到: {filePath}");
        }

        
        // 对象池引用
        private MapNodePool _nodePool;

        private Vector3[] _v3direction = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        private List<TagType> TagList = new();

        public Dictionary<Vector2Int, MapNode> MapData
        {
            get => _mapData;
            private set => _mapData = value;
        } // 地图数据属性


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            // 获取或创建对象池
            _nodePool = FindObjectOfType<MapNodePool>();
            if (_nodePool == null)
            {
                var poolObj = new GameObject("MapNodePool");
                _nodePool = poolObj.AddComponent<MapNodePool>();
            }
            scanInfoConfig = Resources.Load<MapScanInfoConfig>("Scene/" + gameObject.scene.name + "/MapScanInfoConfig");
            if (scanInfoConfig == null)
            {
                Debug.LogError("未找到MapInfoConfig");
            }
            else
            {
                TagList = scanInfoConfig.tagList;
                offsetDistance = scanInfoConfig.offsetDistance;
                startY = scanInfoConfig.startY;
            }
        }

        private void Start()
        {
            //初始化TagMap
            foreach (var tags in TagList)
                if (TagMap.ContainsKey(tags.ToString()))
                    Debug.LogError("TagMap中已存在该标签");
                else
                    TagMap.Add(tags.ToString(), tags);

            //初始化MapData
            MapData = new Dictionary<Vector2Int, MapNode>();
            ScanAllMap();
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.T)) PrintMap();
            if (Input.GetKeyDown(KeyCode.R)) ScanAllMap();
            if (Input.GetKeyDown(KeyCode.Q)) MapToJosn();
            if (Input.GetKeyDown(KeyCode.Z)) MapToNetwork();

            if (Input.GetKeyDown(KeyCode.F))
            {
                var info = SearchPath(TStart, TEnd, true);
                Debug.Log(info);
            }
        }

        private void OnDestroy()
        {
            // 当对象销毁时，将所有节点归还到对象池
            if (_nodePool != null)
                foreach (var node in MapData.Values)
                    _nodePool.Return(node);
            Instance = null;
        }

        private void PrintMap()
        {
            foreach (var mapNode in MapData)
                if (mapNode.Value != null)
                    Debug.Log(mapNode.Key + " : " + mapNode.Value);
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
            return _mapData.ContainsKey(pos);
        }

        #region 基础扫图

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

            InitNeighbor();
        }

        private void ScanCollider(Vector3 v3Pos, int i, int j)
        {
            var key = new Vector2Int(i, j);
            if (MapData.ContainsKey(key)) return;
            var node = _nodePool.Get();
            node.CurrentPos = key;
            // var flag = false;
            var colliderCount = Physics.OverlapBoxNonAlloc(v3Pos, new Vector3(0.4f, 0.4f, 0.4f), _hitColliders);
            for (var k = 0; k < colliderCount; k++)
            {
                if (_hitColliders[k].CompareTag(nameof(TagType.Wall)))
                {
                    _nodePool.Return(node);
                    return;
                }

                if (TagMap.ContainsKey(_hitColliders[k].tag))
                {
                    //node.CurrentTag.Add(TagMap[_hitColliders[k].tag]); //添加标签
                    var baseObject = _hitColliders[k].GetComponent<BaseObject>();
                    if (baseObject == null)
                    {
                        Debug.LogError("Target 不存在 BaseObjetc");
                    }
                    baseObject.VirtualPosition = GetVirtualCoord(baseObject.transform.position);
                    if(!_hitColliders[k].CompareTag(nameof(TagType.Player)) && !_hitColliders[k].CompareTag(nameof(TagType.Enemy)))
                    {
                        baseObject.Id = $"{_hitColliders[k].tag}{_count++}";
                    }
                    node.CurrentTagOb.Add(baseObject, TagMap[_hitColliders[k].tag]);
                    // flag = true;
                }
            }

            // if (!flag)
            // {
            //     //node.CurrentTag.Add(TagType.Nothing); //添加标签
            //     node.CurrentTagOb.Add(TagType.Nothing, null);
            // }
            MapData.Add(key, node); //添加节点
        }

        private void InitNeighbor()
        {
            foreach (var node in _mapData)
            foreach (var dir in _direction)
            {
                var keyDir = node.Key + dir;
                if (MapData.TryGetValue(keyDir, out var neighborNode)) node.Value.AddNeighbor(neighborNode);
            }
        }


        #endregion

        #region 坐标转换

        /// <summary>
        ///     获取虚拟坐标
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <returns>虚拟坐标</returns>
        public Vector2Int GetVirtualCoord(Vector3 pos)
        {
            return new Vector2Int(Mathf.FloorToInt(pos.x) + offsetDistance, Mathf.FloorToInt(pos.z) + offsetDistance);
        }

        /// <summary>
        ///     获取真实坐标
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
        ///     判断是否可行走
        /// </summary>
        /// <param name="v3Pos">真实坐标</param>
        /// <returns>返回是否可行走</returns>
        public bool IsWalkable(Vector3 v3Pos)
        {
            return CompareTag(v3Pos, TagType.Nothing);
        }

        /// <summary>
        ///     判断是否可行走
        /// </summary>
        /// <param name="pos">虚拟坐标</param>
        /// <returns>返回是否可行走</returns>
        public bool IsWalkable(Vector2Int pos)
        {
            return CompareTag(pos, TagType.Nothing);
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag
        /// </summary>
        /// <param name="v3Pos">真实坐标</param>
        /// <param name="type">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector3 v3Pos, TagType type)
        {
            var virtualCoord = GetVirtualCoord(v3Pos);
            return CompareTag(virtualCoord, type);
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag
        /// </summary>
        /// <param name="pos">真实坐标</param>
        /// <param name="type">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector2Int pos, TagType type)
        {
            if (!_mapData.ContainsKey(pos)) return false;
            // ScanPoint(pos, MapData[pos]);
            if (type == TagType.Nothing && MapData[pos].CurrentTagOb.Count == 0)
            {
                return true;
            }
            foreach (var tagType in MapData[pos].CurrentTagOb.Values)
                if (tagType == type)
                    return true;
            return false;
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag中的一个
        /// </summary>
        /// <param name="v3Pos">真实坐标</param>
        /// <param name="types">比较类型</param>
        /// <returns>坐标下的点是否是所需的tag</returns>
        public bool CompareTag(Vector3 v3Pos, List<TagType> types)
        {
            var virtualCoord = GetVirtualCoord(v3Pos);
            return CompareTag(virtualCoord, types);
        }

        /// <summary>
        ///     比较该坐标下的点是否是所需的tag中的一个
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

            //ScanPoint(pos, MapData[pos]);
            foreach (var tagType in MapData[pos].CurrentTagOb.Values)
                if (types.Contains(tagType))
                    return true;
            return false;
        }

        #endregion

        #region 判断路径存在

        //预计使用AStar算法进行实现

        public PathInfo SearchPath(Vector3 v3StartPos, Vector3 v3EndPos, bool isThinkAboutExplosion)
        {
            return SearchPath(GetVirtualCoord(v3StartPos), GetVirtualCoord(v3EndPos), isThinkAboutExplosion);
        }

        /// <summary>
        ///     道具吸引力权重，当路径经过道具节点时，F值会按此权重降低
        /// </summary>
        [Header("道具寻路设置")] [Tooltip("道具对寻路F值的降低权重（0=不降低，值越大吸引力越强）")]
        public float propAttractionWeight = 0.3f;

        /// <summary>
        ///     使用A*算法搜索路径
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="endPos">目标位置</param>
        /// <param name="isThinkAboutExplosion">是否考虑爆炸</param>
        /// <returns>路径信息列表，如果找不到路径则返回null</returns>
        public PathInfo SearchPath(Vector2Int startPos, Vector2Int endPos, bool isThinkAboutExplosion)
        {
            if (!MapData.ContainsKey(startPos) || !MapData.ContainsKey(endPos)) // 检查起点和终点是否在地图数据中
            {
                Debug.LogError($"起始点{startPos}或终点{endPos}不存在");
                return null;
            }
            // 初始化数据结构
            var openList = new MinHeap<MapNode>(CompareNodeByF); //开放列表
            var closeList = new HashSet<MapNode>(); //闭合列表
            var cameFrom = new Dictionary<MapNode, MapNode>(); //路径追溯
            var gScore = new Dictionary<MapNode, float>(); //从起点到当前节点的实际代价
            var startNode = MapData[startPos]; // 获取起点和终点节点
            var endNode = MapData[endPos];
            gScore[startNode] = 0; // 初始化起点的g值和f值
            startNode.F = Heuristic(startNode, endNode);
            openList.Add(startNode);  // 将起点加入开放列表
            while (openList.Count > 0)
            {
                var currentNode = openList.Pop(); // 获取f值最小的节点
                if (currentNode == endNode) // 如果到达目标节点，构建并返回路径
                    return ReconstructPath(cameFrom, currentNode, startPos, endPos);
                closeList.Add(currentNode); // 将当前节点加入关闭列表
                foreach (var neighbor in currentNode.NeighborNodes) // 遍历当前节点的所有邻居
                {
                    if (closeList.Contains(neighbor)) continue; // 如果邻居在关闭列表中，跳过
                    if (!IsWalkable(neighbor.CurrentPos)) continue; //如果不可行走，跳过
                    if (isThinkAboutExplosion && neighbor != endNode &&
                        BombPos.Instance.IsInExportArea(GetRealCoord(neighbor.CurrentPos)))
                        continue; // 检查邻居是否在爆炸范围内，如果是则跳过（除非是终点）
                    // 计算从起点经过当前节点到邻居的代价
                    var neighborGScore = gScore[currentNode] + DistanceBetween(currentNode, neighbor);
                    // 道具吸引力：如果邻居节点上有道具，降低g值使路径更倾向经过此处
                    if (neighbor.HasTag(TagType.Props))
                        neighborGScore -= propAttractionWeight;
                    // 如果邻居不在开放列表中，或者找到更好的路径
                    if (!openList.Contains(neighbor) || neighborGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = currentNode; // 记录路径
                        gScore[neighbor] = neighborGScore;// 更新g值和f值
                        neighbor.F = gScore[neighbor] + Heuristic(neighbor, endNode);
                        //if (!openList.Contains(neighbor)) openList.Add(neighbor);// 如果邻居不在开放列表中，加入开放列表
                        openList.Add(neighbor);// 如果邻居不在开放列表中，加入开放列表

                    }
                }
            }
            // 无法找到路径
            return null;
        }

        /// <summary>
        ///     查找指定位置周围最近的可行走位置
        /// </summary>
        private Vector2Int FindNearestWalkablePosition(Vector2Int pos)
        {
            // 使用BFS从目标位置向外搜索最近的可行走点
            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // 检查当前位置是否可行走
                if (IsWalkable(current)) return current;

                // 检查四个方向的邻居
                foreach (var direction in _direction)
                {
                    var next = current + direction;

                    // 检查是否在地图范围内且未访问过
                    if (MapData.ContainsKey(next) && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return Vector2Int.zero; // 未找到可行走位置
        }

        /// <summary>
        ///     路径回溯，构建完整路径
        /// </summary>
        private PathInfo ReconstructPath(Dictionary<MapNode, MapNode> cameFrom, MapNode current, Vector2Int startPos,
            Vector2Int endPos)
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
                Type = MapData[endPos].CurrentTagOb.Count > 0 ? MapData[endPos].CurrentTagOb.First().Value : TagType.Nothing
            };
            return new PathInfo(startPos, targetInfo, path);
        }

        /// <summary>
        ///     计算两个节点之间的距离（移动代价）
        /// </summary>
        private float DistanceBetween(MapNode a, MapNode b)
        {
            // 在网格地图中，相邻节点之间的距离通常为1
            return Vector2Int.Distance(a.CurrentPos, b.CurrentPos);
        }

        /// <summary>
        ///     启发函数，估计从当前节点到目标节点的代价
        /// </summary>
        private float Heuristic(MapNode a, MapNode b)
        {
            // 使用曼哈顿距离作为启发函数
            return Mathf.Abs(a.CurrentPos.x - b.CurrentPos.x) + Mathf.Abs(a.CurrentPos.y - b.CurrentPos.y);
        }

        /// <summary>
        ///     比较两个节点的f值，用于优先队列排序
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


        /// <summary>
        /// 从指定位置开始搜索符合标签类型的节点
        /// </summary>
        /// <param name="startPos">搜索的起始位置</param>
        /// <param name="tagTypes">需要匹配的标签类型列表</param>
        /// <returns>返回符合条件的TargetStepInfo列表，如果没有找到则返回null</returns>
        public List<TargetStepInfo> SearchTags(Vector2Int startPos, List<TagType> tagTypes) {
            // 检查起始位置是否存在于地图数据中
            if (!MapData.ContainsKey(startPos)) { //不存在位置时，返回空
                Debug.LogWarning("此位置不存在" + startPos);
                return null;
            }
            // 初始化队列用于广度优先搜索
            var que = new Queue<Vector2Int>();
            // 记录已访问过的位置，避免重复处理
            var visited = new HashSet<Vector2Int>(); 
            // 将起始位置加入队列并标记为已访问
            que.Enqueue(startPos);
            visited.Add(startPos);
            // 记录搜索步数
            var count = 0;
            // 存储搜索结果
            var result = new List<TargetStepInfo>();
            // 使用广度优先搜索算法遍历地图
            while (que.Count > 0)
            {
                count++; // 每处理一层，步数加1
                // 取出队列中的当前位置
                var current = que.Dequeue();

                // 获取当前位置的地图节点
                var currentNode = MapData[current];
                // 遍历当前节点的所有相邻节点
                foreach (var currentNodeMapNode in currentNode.NeighborNodes)
                {
                    // 如果相邻节点已经访问过，则跳过
                    if (visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                    // 检查相邻节点的标签是否符合要求，且不在爆炸区域内
                    if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes) &&
                        !BombPos.Instance.IsInExportArea(GetRealCoord(currentNodeMapNode.CurrentPos)))
                    {
                        // 将符合条件的位置添加到结果列表中
                        result.Add(TargetStepInfoPool.Instance.Get(currentNode.CurrentPos, count));
                        // 标记为已访问
                        visited.Add(currentNodeMapNode.CurrentPos);
                        // 继续检查下一个相邻节点
                        continue;
                    }
                    // 标记为已访问并加入队列继续搜索
                    visited.Add(currentNodeMapNode.CurrentPos);
                    que.Enqueue(currentNodeMapNode.CurrentPos);
                }
            }

            // 如果没有找到符合条件的位置，返回null，否则返回结果列表
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


    /// <summary>
    /// 从指定位置开始搜索符合特定标签类型的区域
    /// </summary>
    /// <param name="startPos">搜索的起始位置</param>
    /// <param name="tagTypes">需要匹配的标签类型列表</param>
    /// <param name="maxArea">最大搜索区域范围</param>
    /// <returns>返回符合条件的目标步骤信息列表，如果没有找到则返回null</returns>
        public List<TargetStepInfo> SearchTags(Vector2Int startPos, List<TagType> tagTypes, int maxArea) {
            // 检查起始位置是否存在于地图数据中
            if (!MapData.ContainsKey(startPos)) //不存在位置时，返回空
            {
                Debug.LogWarning("此位置不存在" + startPos);
                return null;
            }

            // 初始化队列和已访问集合
            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            // 将起始位置加入队列和已访问集合
            que.Enqueue(startPos);
            visited.Add(startPos);
            var count = 1; // 记录当前层级的节点数量
            var result = new List<TargetStepInfo>();
            // 按层级进行广度优先搜索
            for (var i = 0; i < maxArea; i++)
            {
                var bfsCount = count; // 当前层级的节点数量
                count = 0; // 重置下一层级的节点计数器
                // 遍历当前层级的所有节点
                for (var j = 0; j < bfsCount; j++)
                {
                    var current = que.Dequeue(); // 取出队列中的第一个节点
                    var currentNode = MapData[current];
                    foreach (var currentNodeMapNode in currentNode.NeighborNodes)
                    {
                        if (visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                        if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes) && !BombPos.Instance.IsInExportArea(
                                GetRealCoord(currentNodeMapNode.CurrentPos)))
                        {
                            result.Add(TargetStepInfoPool.Instance.Get(currentNode.CurrentPos, i));
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
            if (!MapData.ContainsKey(startPos)) //不存在位置时，返回空
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
                foreach (var currentNodeMapNode in currentNode.NeighborNodes)
                {
                    if (visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                    if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes) && !BombPos.Instance.IsInExportArea(
                            GetRealCoord(currentNodeMapNode.CurrentPos)))
                        return TargetStepInfoPool.Instance.Get(currentNode.CurrentPos, count);
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
            if (!MapData.ContainsKey(startPos)) //不存在位置时，返回空
            {
                Debug.LogWarning("此位置不存在" + startPos);
                return null;
            }

            var que = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            que.Enqueue(startPos);
            visited.Add(startPos);
            var count = 1;
            for (var i = 0; i < maxArea; i++)
            {
                var bfsCount = count;
                count = 0;
                for (var j = 0; j < bfsCount; j++)
                {
                    var current = que.Dequeue();
                    var currentNode = MapData[current];
                    foreach (var currentNodeMapNode in currentNode.NeighborNodes)
                    {
                        if (visited.Contains(currentNodeMapNode.CurrentPos)) continue;
                        if (CompareTag(currentNodeMapNode.CurrentPos, tagTypes) && !BombPos.Instance.IsInExportArea(
                                GetRealCoord(currentNodeMapNode.CurrentPos)))
                            return TargetStepInfoPool.Instance.Get(currentNode.CurrentPos, i);
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
            var startPos = GetVirtualCoord(v3StartPos);
            return GetRandomPointInArea(startPos, area);
        }


        public TargetStepInfo GetRandomPointInArea(Vector2Int startPos, int area)
        {
            if (!IsValidPosition(startPos))
            {
                Debug.LogError("不合法的位置: " + startPos);
                return null;
            }

            var tryCount = offsetDistance * offsetDistance;
            var minx = Mathf.CeilToInt(MathF.Min(Mathf.Max(startPos.x - area + 1, 0), offsetDistance * 2));
            var maxx = Mathf.CeilToInt(Mathf.Min(startPos.x + area - 1, offsetDistance * 2));
            var miny = Mathf.CeilToInt(MathF.Min(Mathf.Max(startPos.y - area + 1, 0), offsetDistance * 2));
            var maxy = Mathf.CeilToInt(Mathf.Min(startPos.y + area - 1, offsetDistance * 2));
            var point = new Vector2Int();
            while (tryCount > 0)
            {
                point.x = Random.Range(minx, maxx);
                point.y = Random.Range(miny, maxy);
                if (IsWalkable(point) && !BombPos.Instance.IsInExportArea(
                        GetRealCoord(point)))
                    return new TargetStepInfo(point, 0);
                tryCount--;
            }

            return null;
        }

        #endregion

        #region 同步地图数据



        public bool AddItem(Vector3 pos, BaseObject item, TagType type)
        {
            return AddItem(GetVirtualCoord(pos), item, type);
        }

        
        public bool AddItem(Vector2Int pos, BaseObject item, TagType type)
        {
            if (_mapData.TryGetValue(pos, out var mapNode))
            {
                return mapNode.AddItem(item, type);
            }
            else
            {
                Debug.LogWarning("此位置不存在" + pos);
                return false;
            }
        }

        public bool HasTag(Vector3 pos, TagType type)
        {
            return HasTag(GetVirtualCoord(pos), type);
        }
        
        public bool HasTag(Vector2Int pos, TagType type)
        {
            if (!_mapData.TryGetValue(pos, out var mapNode))
            {
                Debug.LogWarning("此位置不存在" + pos);
                return false;  
            }
            return mapNode.HasTag(type);
        }
        
        public List<BaseObject> GetItem(Vector2Int pos, TagType type)
        {
            if (!_mapData.TryGetValue(pos, out var mapNode))
            {
                Debug.LogWarning("此位置不存在" + pos);
                return null;
            }
            return mapNode.GetItem(type);
        }

        public bool RemoveItem(Vector3 pos, BaseObject item)
        {
            return RemoveItem(GetVirtualCoord(pos), item);
        }
        
        public bool RemoveItem(Vector2Int pos, BaseObject item)
        {
            if (!_mapData.TryGetValue(pos, out var mapNode))
            {
                Debug.LogWarning("此位置不存在" + pos);
                return false;
            }
            return mapNode.RemoveItem(item);
        }

        public bool UpdateItem(Vector3 newPos, Vector3 oldPos, BaseObject item, TagType type)
        {
            return UpdateItem(GetVirtualCoord(newPos), GetVirtualCoord(oldPos), item, type);
        }
        
        public bool UpdateItem(Vector2Int newPos, Vector2Int oldPos, BaseObject item, TagType type)
        {
            if (!IsValidPosition(newPos) || !IsValidPosition(oldPos))
            {
                Debug.LogError("不合法的位置: " + newPos + " 或 " + oldPos);
                return false;
            }

            if (newPos == oldPos)
            {
                Debug.LogWarning("位置相同，无需更新");
                return AddItem(newPos, item, type);
            }

            if (!RemoveItem(oldPos, item))
            {
                Debug.LogError("无法移除旧位置的项目: " + oldPos);
            }
            return AddItem(newPos, item, type);
        }
        
        public Dictionary<BaseObject, TagType> GetMapDataTarget(Vector3 pos)
        {
            return GetMapDataTarget(GetVirtualCoord(pos));
        } 
        
        public Dictionary<BaseObject, TagType> GetMapDataTarget(Vector2Int pos)
        {
            _mapData.TryGetValue(pos, out var mapNode);
            if (mapNode == null)
            {
                return null;
            }
            return _mapData[pos].GetTargetDic();
        }
        
        
        
        #endregion
        
        [Serializable]
        public class SerializableTag
        {
            public string objectId; // 存储 BaseObject 的 Id
            public string tagType;  // 使用字符串存储 TagType
    
            public SerializableTag(string id, TagType type)
            {
                tagType = type.ToString();
                objectId = id;
            }
        }

        [Serializable]
        public class SerializableVector2Int
        {
            public int x;
            public int y;
    
            public SerializableVector2Int(Vector2Int pos)
            {
                x = pos.x;
                y = pos.y;
            }
    
            public Vector2Int ToVector2Int()
            {
                return new Vector2Int(x, y);
            }
        }

        [Serializable]
        public class SerializableMapNode
        {
            public int x;
            public int y;
            public List<SerializableTag> tags;
            public List<SerializableVector2Int> neighbors;
    
            public SerializableMapNode(Vector2Int pos, List<KeyValuePair<BaseObject, TagType>> tags, List<Vector2Int> neighbors)
            {
                this.x = pos.x;
                this.y = pos.y;
                this.tags = tags.Select(t => new SerializableTag(t.Key.Id, t.Value)).ToList();
                this.neighbors = neighbors.Select(n => new SerializableVector2Int(n)).ToList();
            }
        }

        [Serializable]
        public class SerializableMapData
        {
            public List<SerializableMapNode> nodes;
    
            public SerializableMapData(Dictionary<Vector2Int, MapNode> mapData)
            {
                nodes = new List<SerializableMapNode>();
                foreach (var kvp in mapData)
                {
                    var neighborPositions = kvp.Value.NeighborNodes.Select(n => n.CurrentPos).ToList();
                    nodes.Add(new SerializableMapNode(kvp.Key, kvp.Value.CurrentTagOb.ToList(), neighborPositions));
                }
            }
        }
    }
}