using System;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Map
{
    [Serializable]
    public class MapNode
    {
        /// <summary>
        ///     存放当前的位置
        /// </summary>
        private Vector2Int _currentPos;


        /// <summary>
        ///     存放当前所处地的tag列表
        /// </summary>
        // private HashSet<TagType> _currentTag = new();
        
        private Dictionary<TagType, BaseObject> _currentCurrentTagOb = new();

        /// <summary>
        ///     存放图的节点
        /// </summary>
        private List<MapNode> _neighborNodes = new();

        /// <summary>
        ///     存放图的节点
        /// </summary>
        public List<MapNode> NeighborNodes
        {
            get => _neighborNodes;
            set => _neighborNodes = value;
        }

        /// <summary>
        ///     存放当前的位置
        /// </summary>
        public Vector2Int CurrentPos
        {
            get => _currentPos;
            set => _currentPos = value;
        }

        public Dictionary<TagType, BaseObject> CurrentTagOb
        {
            get => _currentCurrentTagOb;
            set => _currentCurrentTagOb = value;
        }

        // /// <summary>
        // ///  存放当前所处地的tag列表
        // /// </summary>
        // public HashSet<TagType> CurrentTag
        // {
        //     get => _currentTag;
        //     set => _currentTag = value;
        // }

        //用于存放代价，A*算法使用
        public float F { get; set; }


        public int GetNeighborCount()
        {
            return NeighborNodes.Count;
        }

        public void AddNeighbor(MapNode node)
        {
            NeighborNodes.Add(node);
        }
    }
}