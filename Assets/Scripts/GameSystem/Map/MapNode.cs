using System;
using System.Collections.Generic;
using config;
using UnityEngine;

namespace GameSystem.Map
{
    [Serializable]
    public class MapNode
    {
        /// <summary>
        /// 存放图的节点
        /// </summary>
        private List<MapNode> _mapNodes = new List<MapNode>();
        /// <summary>
        /// 存放图的节点
        /// </summary>
        public List<MapNode> MapNodes { get => _mapNodes; set => _mapNodes = value; }

        /// <summary>
        /// 存放当前的位置
        /// </summary>
        private Vector2Int _currentPos = new Vector2Int();
        
        /// <summary>
        /// 存放当前的位置
        /// </summary>
        public Vector2Int CurrentPos { get => _currentPos; set => _currentPos = value; }
        
        
        /// <summary>
        /// 存放当前所处地的tag列表
        /// </summary>
        private HashSet<TagType> _currentTag = new HashSet<TagType>();
        
        /// <summary>
        /// 存放当前所处地的tag列表
        /// </summary>
        public HashSet<TagType> CurrentTag { get => _currentTag; set => _currentTag = value; }
        
        //用于存放代价，A*算法使用
        public float F { get; set; }


        public int GetNeighborCount()
        {
            return MapNodes.Count;
        }

        public void AddNeighbor(MapNode node)
        {
            MapNodes.Add(node);
        }
    }
}