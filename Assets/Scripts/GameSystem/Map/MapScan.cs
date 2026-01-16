using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GameSystem.Map
{
    /// <summary>
    /// 地图扫描，要求地图关于（0，0）对称
    /// 请将该组件放置到起始点位置
    /// </summary>
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 地图扫描类，用于扫描和管理游戏地图数据
/// </summary>
    public class MapScan : MonoBehaviour
    {
        [Header("地图扫描设置")]
        [Tooltip("扫描起始参照点")]
        public float startY = 0.5f;                    // 扫描的起始Y坐标
        [Tooltip("偏移偏移距离（包含（0.5,0.5），到边界距离)")]
        public int offsetDistance = 15;                // 扫描偏移距离，决定扫描范围
        public List<TagPointPairs> TagPointPairsList = new List<TagPointPairs>();  // 标签点对列表
        public Dictionary<string,char> TagPointPairsMap = new Dictionary<string, char>();  // 标签点对映射字典
        public MapData MapData { get; set; }          // 地图数据属性

        public MapScanConfig ScanConfig;              // 地图扫描配置
        
        [Header("局部扫描调试")]
        
        [Tooltip("局部扫描的中心位置")]
        public Vector3 referencePos = new Vector3(0.5f,0.5f,0.5f);  // 局部扫描的中心位置
        [Tooltip("局部扫描的半径")]
        public int referenceOffset = 5;               // 局部扫描的半径

        [Header("打印调试")]
        public bool isPrint = true;                   // 是否打印调试信息

        /// <summary>
        /// 唤醒方法，用于初始化配置
        /// </summary>
        private void Awake()
        {
            ScanConfig =  Resources.Load<MapScanConfig>(gameObject.scene.name + "/MapScanConfig");
            if (ScanConfig == null)
            {
                Debug.LogError("未找到MapScanConfig");
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
            MapData = new MapData(offsetDistance,TagPointPairsList);
            foreach (TagPointPairs tagPointPairs in TagPointPairsList)
            {
                if (TagPointPairsMap.ContainsKey(tagPointPairs.tag.ToString()))
                {
                    Debug.LogError("重复的Tag");
                }
                else
                {
                    TagPointPairsMap.Add(tagPointPairs.tag.ToString(), tagPointPairs.point);
                }
            }
            
            // 扫描地图
            ScanAllMap();
            MapData.PrintMap();
        }
        
        /// <summary>
        /// 扫描地图，将地图数据存入MapData.Map中
        /// </summary>
        public void ScanAllMap()
        {
            for (int j = 0; j < offsetDistance * 2; j++)
            {
                for (int i = 0; i < offsetDistance * 2; i++)
                {
                    // print(i + "," + j);
                    Vector3 pos = new Vector3(i - offsetDistance + 0.5f, startY, j - offsetDistance + 0.5f);
                    bool flag = false;
                    Collider[] hiColliders = Physics.OverlapBox(pos, new Vector3(0.4f, 0.4f, 0.4f));
                    foreach (var colldier in hiColliders)
                    {
                        if (TagPointPairsMap.ContainsKey(colldier.tag))
                        {
                            if (flag)
                            {
                                Debug.LogError("出现Tag重复或者出现一个地方有两个相同tag\n" + "pos:" + pos + "tag:" + colldier.tag + "现存tag =" + MapData.Map[i,j]);
                            }
                            else
                            {
                                flag = true;
                                MapData.Map[i,j] = TagPointPairsMap[colldier.tag];//赋值
                            }
                        }
                    }

                    if (!flag)
                    {
                        MapData.Map[i,j] = TagPointPairsMap[ObjectType.Nothing.ToString()];//当无标签时
                    }
                }
            }
        }

        public void ScanArea(Vector3 refPos,int refOffset)
        {
            Vector2Int virtualCoord = GetVirtualCoord(refPos);
            ScanArea(virtualCoord, refOffset);
        }
        
        
        public void ScanArea(Vector2Int refPos,int refOffset)
        {
            int startX = refPos.x - refOffset + 1;
            int endX = refPos.x + refOffset - 1;
            int startZ = refPos.y - refOffset + 1;
            int endZ = refPos.y + refOffset - 1;
            
            print("扫描中心坐标:" + refPos.x + "," + refPos.y);
            print("扫描范围:x = " + startX + "," + endX + ", z = " + startZ + "," + endZ);
            for (int j = Mathf.Min(Mathf.Max(startZ, 0), offsetDistance * 2 - 1); j < offsetDistance * 2 && j <= endZ; j++)
            {
                for (int i = Mathf.Min(Mathf.Max(startX, 0), offsetDistance * 2 - 1); i < offsetDistance * 2 && i <= endX; i++)
                { 
                    Vector3 pos = new Vector3(i - offsetDistance + 0.5f, startY, j - offsetDistance + 0.5f);
                    bool flag = false;
                    Collider[] hiColliders = Physics.OverlapBox(pos, new Vector3(0.4f, 0.4f, 0.4f));
                    foreach (var colldier in hiColliders)
                    {
                        if (TagPointPairsMap.ContainsKey(colldier.tag))
                        {
                            if (!flag)
                            {
                                flag = true;
                                MapData.Map[i,j] = TagPointPairsMap[colldier.tag];//赋值
                            }
                        }
                    }

                    if (!flag)
                    {
                        MapData.Map[i,j] = 'E';//当无标签时
                    }
                }
            }
        }
        
        
        

        /// <summary>
        /// 获取虚拟坐标
        /// </summary>
        /// <param name="pos"> 真实坐标 </param>
        /// <returns></returns>
        public Vector2Int GetVirtualCoord(Vector3 pos)
        {
            return new Vector2Int(Mathf.CeilToInt(pos.x + offsetDistance), Mathf.CeilToInt(pos.z + offsetDistance));
        }

        public Vector3 GetRealCoord(Vector2Int virtualCoord)
        {
            return new Vector3(virtualCoord.x - offsetDistance + 0.5f, startY, virtualCoord.y - offsetDistance + 0.5f);
        }

        public bool IsWalkable(Vector3 pos)
        {
            Vector2Int virtualCoord = GetVirtualCoord(pos);
            return IsWalkable(virtualCoord);
        }

        public bool IsWalkable(Vector2Int virtualCoord)
        {
            return MapData.Map[virtualCoord.x,virtualCoord.y] == 'E';
        }

        public bool CompareTag(Vector3 pos, ObjectType type)
        {
            Vector2Int virtualCoord = GetVirtualCoord(pos);
            return CompareTag(virtualCoord, type);
        }

        public bool CompareTag(Vector2Int virtualCoord, ObjectType type)
        {
            return MapData.Map[virtualCoord.x,virtualCoord.y] == TagPointPairsMap[type.ToString()];
        }

        public bool CompareTags(Vector3 pos, ObjectType[] types)
        {
            Vector2Int virtualCoord = GetVirtualCoord(pos);
            return CompareTags(virtualCoord, types);
        }

        public bool CompareTags(Vector2Int virtualCoord, ObjectType[] types)
        {
            foreach (var type in types)
                if (MapData.Map[virtualCoord.x, virtualCoord.y] == TagPointPairsMap[type.ToString()])
                    return true;
            return false;
        }
        
        public bool CompareTags(Vector3 pos, List<ObjectType> types)
        {
            Vector2Int virtualCoord = GetVirtualCoord(pos);
            return CompareTags(virtualCoord, types);
        }

        public bool CompareTags(Vector2Int virtualCoord, List<ObjectType> types)
        {
            foreach (var type in types)
                if (MapData.Map[virtualCoord.x, virtualCoord.y] == TagPointPairsMap[type.ToString()])
                    return true;
            return false;
        }
        
        
        
        private void Update()
        {
            if (!isPrint)
            {
                ScanArea(referencePos,referenceOffset);
                MapData.PrintMap();
                isPrint = true;
            }
        }
    }
    


}