using System;
using GameSystem.GameScene.MainMenu.Map;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
{
    public class BaseObject : MonoBehaviour
    {
        /// <summary>
        ///     物品ID，角色自命名，其余默认使用物品ID
        /// </summary>
        [SerializeField]
        [Tooltip("角色ID，角色自命名，其余默认使用物品ID")] protected string id;
        [SerializeField]
        [Tooltip("虚拟坐标")]
        protected Vector2Int virtualPosition;
        
        public Vector2Int VirtualPosition
        {
            get => virtualPosition;
            set => virtualPosition = value;
        }

        public string Id
        {
            get => id;
            set => id = value;
        }
        

    }
}