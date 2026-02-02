using UnityEngine;

namespace GameSystem
{
    public class BaseObject : MonoBehaviour
    {
        /// <summary>
        ///     物品ID，角色自命名，其余默认使用物品ID
        /// </summary>
        [Tooltip("角色ID，角色自命名，其余默认使用物品ID")] public string id;
    }
}