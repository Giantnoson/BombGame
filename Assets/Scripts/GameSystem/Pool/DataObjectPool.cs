using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Pool
{
    /// <summary>
    ///     通用对象池基类，用于非MonoBehaviour对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public abstract class DataObjectPool<T> : MonoBehaviour where T : class, new()
    {
        [Header("对象池设置")] [Tooltip("初始池大小")] [SerializeField]
        protected int initialPoolSize = 100;

        [Tooltip("最大池大小")] [SerializeField] protected int maxPoolSize = 500;

        protected Queue<T> objectPool = new();

        protected virtual void Awake()
        {
            InitializeSingleton();
            InitPool();
        }

        /// <summary>
        ///     初始化单例模式
        /// </summary>
        protected abstract void InitializeSingleton();

        /// <summary>
        ///     初始化对象池
        /// </summary>
        protected virtual void InitPool()
        {
            for (var i = 0; i < initialPoolSize; i++)
            {
                var obj = CreateNewObject();
                objectPool.Enqueue(obj);
            }
        }

        /// <summary>
        ///     创建新的对象
        /// </summary>
        protected virtual T CreateNewObject()
        {
            return new T();
        }

        /// <summary>
        ///     从对象池中获取对象
        /// </summary>
        public virtual T Get()
        {
            T obj;

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

                obj = CreateNewObject();
            }

            return obj;
        }

        /// <summary>
        ///     将对象返回到对象池
        /// </summary>
        public virtual void Return(T obj)
        {
            ResetObject(obj);

            // 检查是否超过最大池大小
            if (objectPool.Count < maxPoolSize) objectPool.Enqueue(obj);
            // 如果池已满，对象将被垃圾回收器回收
        }

        /// <summary>
        ///     重置对象状态
        /// </summary>
        protected virtual void ResetObject(T obj)
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
            objectPool.Clear();
        }
    }
}