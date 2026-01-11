
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game_props
{
    /// <summary>
    /// 对象池基类，提供通用的对象池功能
    /// </summary>
    public abstract class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
    {
        [Header("对象池设置")]
        [Tooltip("预制体")]
        public GameObject prefab;
        [Tooltip("初始池大小")]
        public int initialPoolSize = 20;
        [Tooltip("池扩展大小")]
        public int poolExpandSize = 10;

        protected Queue<GameObject> objectPool = new Queue<GameObject>();
        protected Transform poolContainer;

        protected virtual void Awake()
        {
            InitializeSingleton();
            ValidatePrefab();
            CreatePoolContainer();
            InitPool();
        }

        /// <summary>
        /// 初始化单例模式
        /// </summary>
        protected abstract void InitializeSingleton();

        /// <summary>
        /// 验证预制体是否有效
        /// </summary>
        protected virtual void ValidatePrefab()
        {
            if (prefab == null)
            {
                Debug.LogError($"{typeof(T).Name}预制体为空");
                return;
            }

            Debug.Log($"{typeof(T).Name}预制体加载成功");

            T component = prefab.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"{typeof(T).Name}预制体上没有{typeof(T).Name}组件");
            }
        }

        /// <summary>
        /// 创建对象池容器
        /// </summary>
        protected virtual void CreatePoolContainer()
        {
            string containerName = $"{typeof(T).Name}PoolContainer";
            poolContainer = new GameObject(containerName).transform;
            poolContainer.SetParent(transform);
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        protected virtual void InitPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = CreateNewObject();
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
        }

        /// <summary>
        /// 创建新的对象
        /// </summary>
        protected virtual GameObject CreateNewObject()
        {
            return Instantiate(prefab, poolContainer);
        }

        /// <summary>
        /// 从对象池中获取对象
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
                // 扩展对象池
                for (int i = 0; i < poolExpandSize - 1; i++)
                {
                    GameObject newObj = CreateNewObject();
                    newObj.SetActive(false);
                    objectPool.Enqueue(newObj);
                }
                obj = CreateNewObject();
            }

            return obj;
        }

        /// <summary>
        /// 将对象返回到对象池
        /// </summary>
        public virtual void ReturnObject(GameObject obj)
        {
            ResetObject(obj);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            objectPool.Enqueue(obj);
        }

        /// <summary>
        /// 重置对象状态
        /// </summary>
        protected virtual void ResetObject(GameObject obj)
        {
            // 子类可以重写此方法以实现特定的重置逻辑
        }
    }
}
