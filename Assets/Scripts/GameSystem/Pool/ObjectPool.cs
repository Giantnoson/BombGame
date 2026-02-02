using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Pool
{
    /// <summary>
    ///     通用对象池基类，提供基本的对象池功能
    /// </summary>
    /// <typeparam name="T">对象类型，必须继承自MonoBehaviour</typeparam>
    public abstract class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
    {
        [Header("对象池设置")] [Tooltip("对象预制体")] public GameObject prefab;

        [Tooltip("初始池大小")] [SerializeField] protected int initialPoolSize = 20;

        [Tooltip("池扩展大小")] [SerializeField] protected int poolExpandSize = 10;

        [Tooltip("最大池大小")] [SerializeField] protected int maxPoolSize = 100;

        protected Queue<GameObject> objectPool = new();
        protected Transform poolContainer;

        protected virtual void Awake()
        {
            InitializeSingleton();
            ValidatePrefab();
            CreatePoolContainer();
            InitPool();
        }

        /// <summary>
        ///     初始化单例模式
        /// </summary>
        protected abstract void InitializeSingleton();

        /// <summary>
        ///     验证预制体是否有效
        /// </summary>
        protected virtual void ValidatePrefab()
        {
            if (prefab == null)
            {
                Debug.LogError($"{typeof(T).Name}预制体为空");
                return;
            }

            var component = prefab.GetComponent<T>();
            if (component == null) Debug.LogError($"{typeof(T).Name}预制体上没有{typeof(T).Name}组件");
        }

        /// <summary>
        ///     创建对象池容器
        /// </summary>
        protected virtual void CreatePoolContainer()
        {
            var containerName = $"{typeof(T).Name}PoolContainer";
            poolContainer = new GameObject(containerName).transform;
            poolContainer.SetParent(transform);
        }

        /// <summary>
        ///     初始化对象池
        /// </summary>
        protected virtual void InitPool()
        {
            for (var i = 0; i < initialPoolSize; i++)
            {
                var obj = CreateNewObject();
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
        }

        /// <summary>
        ///     创建新的对象
        /// </summary>
        protected virtual GameObject CreateNewObject()
        {
            return Instantiate(prefab, poolContainer);
        }

        /// <summary>
        ///     从对象池中获取对象
        /// </summary>
        protected virtual GameObject GetObjectFromPool()
        {
            GameObject obj;

            if (objectPool.Count > 0)
            {
                obj = objectPool.Dequeue();
            }
            else
            {
                // 检查是否达到最大池大小
                if (objectPool.Count >= maxPoolSize)
                {
                    Debug.LogWarning($"{typeof(T).Name}对象池已达到最大大小 {maxPoolSize}，无法扩展");
                    return null;
                }

                // 扩展对象池
                var expandCount = Mathf.Min(poolExpandSize, maxPoolSize - objectPool.Count);
                for (var i = 0; i < expandCount - 1; i++)
                {
                    var newObj = CreateNewObject();
                    newObj.SetActive(false);
                    objectPool.Enqueue(newObj);
                }

                obj = CreateNewObject();
            }

            return obj;
        }

        /// <summary>
        ///     将对象返回到对象池
        /// </summary>
        public virtual void ReturnObject(GameObject obj)
        {
            ResetObject(obj);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);

            // 检查是否超过最大池大小
            if (objectPool.Count < maxPoolSize)
                objectPool.Enqueue(obj);
            else
                Destroy(obj);
        }

        /// <summary>
        ///     重置对象状态
        /// </summary>
        protected virtual void ResetObject(GameObject obj)
        {
            // 子类可以重写此方法以实现特定的重置逻辑
        }

        /// <summary>
        ///     获取对象池中当前可用的对象数量
        /// </summary>
        public int AvailableCount()
        {
            return objectPool.Count;
        }

        /// <summary>
        ///     清空对象池
        /// </summary>
        public void ClearPool()
        {
            while (objectPool.Count > 0)
            {
                var obj = objectPool.Dequeue();
                if (obj != null) Destroy(obj);
            }
        }
    }
}