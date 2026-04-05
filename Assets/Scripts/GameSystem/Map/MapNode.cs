using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using GameSystem.GameScene;
using UnityEngine;

namespace GameSystem.Map
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
        
        private Dictionary<BaseObject, TagType> _currentTagOb = new();

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

        public Dictionary<BaseObject, TagType> CurrentTagOb
        {
            get => _currentTagOb;
            set => _currentTagOb = value;
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


        #region 添加item

        public bool AddItem(BaseObject item, TagType type)
        {
            return _currentTagOb.TryAdd(item, type);
        }
        
        #endregion
        
        #region 查询item
        
        public bool HasItem(BaseObject item)
        {
            return _currentTagOb.ContainsKey(item);
        }

        public bool HasItem(string itemId)
        {
            return _currentTagOb.Any(x => x.Key.Id == itemId);
        }
        #endregion

        #region 获取item
        
        public List<BaseObject> GetItem(TagType type)
        {
            return _currentTagOb.Where(x => x.Value == type).Select(x => x.Key).ToList();
        }

        public bool TryGetItem(string itemId, out BaseObject item)
        {
            item = _currentTagOb.FirstOrDefault(x => x.Key.Id == itemId).Key;
            return item != null;
        }
        
        #endregion

        #region 移除item

        public bool RemoveItem(BaseObject item)
        {
            return _currentTagOb.Remove(item);
        }
        
        public bool RemoveItem(string itemId)
        {
            return _currentTagOb.Remove(_currentTagOb.FirstOrDefault(x => x.Key.Id == itemId).Key);
        }

        #endregion
        
        
        public bool HasTag(TagType type)
        {
            return _currentTagOb.ContainsValue(type);
        }
        
        public Dictionary<BaseObject, TagType> GetTargetDic()
        {
            return _currentTagOb;
        }
        
        public bool GetTargetDic(string itemId, out KeyValuePair<BaseObject, TagType> kvp)
        {
            kvp = _currentTagOb.FirstOrDefault(x => x.Key.Id == itemId);
            return kvp.Key != null;
        }
        
        
    }
}