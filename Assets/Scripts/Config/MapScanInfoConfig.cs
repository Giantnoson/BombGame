using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    /// <summary>
    /// 基于图的全局扫图
    /// </summary>
    [CreateAssetMenu()]
    public class MapScanInfoConfig :ScriptableObject
    {
        [Tooltip("扫描Y轴起始参照点")]
        public float startY = 0.5f;
        [Tooltip("偏移偏移距离（包含（0.5,0.5），到边界距离)")]
        public int offsetDistance = 15;
        [Tooltip("扫描类型")]
        public List<TagType> tagList = new List<TagType>();
    }

    [Serializable]
    public enum TagType
    {
        Wall,
        Destructible,
        Player,
        Enemy,
        Bomb,
        Nothing
    }
}