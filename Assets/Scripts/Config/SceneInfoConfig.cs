using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class SceneInfoConfig : ScriptableObject
    {
        [SerializeField]
        public List<SceneInfo> sceneInfos;
    }
    
    [Serializable]
    public class SceneInfo
    {
        [Tooltip("场景名称")]
        public string sceneName;
        [Tooltip("场景类型")]
        public GameState state;
        [Tooltip("是否为仅标识")]
        public bool isFlag;
        [Tooltip("叠加模式？")]
        public bool isAdditive; // 是否以叠加方式加载场景
        [Tooltip("持久化场景？")]
        public bool isPersistent; // 是否为持久化场景（不会被卸载）
        [Tooltip("启动时加载？")]
        public bool loadOnStartup; // 是否在启动时加载
    }
    [Serializable]
    public enum GameState
    {
        MainMenu, //主菜单
        Loading, //加载中
        Playing, //游戏进行中
        Paused, //游戏暂停
        GameOver, //游戏结束
        Victory, //游戏胜利
        Settings, //设置界面
    }
}