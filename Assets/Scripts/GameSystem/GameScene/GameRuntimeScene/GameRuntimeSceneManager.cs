using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameSystem.GameScene.GameRuntimeScene
{
    //游戏运行场景管理器负责管理游戏当中的场景初始化，相对于场景中的流
    public class GameRuntimeSceneManager : MonoBehaviour
    {
        public static GameRuntimeSceneManager Instance { get; private set; }
        
        [Header("初始组件")]
        [Tooltip("玩家预制体")]
        
        public List<GameObject> players;
        [Tooltip("NPC预制体")]
        public List<GameObject> npcs;
        [Tooltip("玩家出生点")]
        public List<Transform> Spawns;
        [Tooltip("玩家状态HUD")]
        public List<GameObject> HUDs;
        [Tooltip("玩家名字文本")]
        public List<TextMeshProUGUI> playerNameTexts;
        
        
        
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        
        
        
        
        
        public static void Init()
        {
            // TODO: 初始化游戏场景
            if (GameModeSelect.Instance.IsOnlineMode())
            {
                // TODO: 初始化在线游戏场景
            }
            else
            {
                // TODO: 初始化离线游戏场景
                if (GameModeSelect.Instance.IsSinglePlayer())
                {
                    // TODO: 初始化单人游戏场景
                }
                else
                {
                    // TODO: 初始化多人游戏场景
                }
            }
        }
        public static void Load()
        {
            // TODO: 加载游戏场景
        }
        
        public static void Unload()
        {
            // TODO: 卸载游戏场景
        }

           
    }
}