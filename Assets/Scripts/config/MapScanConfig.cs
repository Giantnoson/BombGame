using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MapScanConfig : ScriptableObject
{
    [Tooltip("扫描Y轴起始参照点")]
    public float startY = 0.5f;
    [Tooltip("偏移偏移距离（包含（0.5,0.5），到边界距离)")]
    public int offsetDistance = 15;
    [Tooltip("tag和point组")]
    public List<TagPointPairs> tagPointPairsList = new List<TagPointPairs>();
}

/// <summary>
/// 地图标签键值对,用于存放地图键值
/// </summary>
[Serializable]
public class TagPointPairs
{
    public ObjectType tag;
    public char point;
}

public enum ObjectType
{
    Wall,
    Destructible,
    Player,
    Enemy,
    Bomb,
    Nothing
    
    
}