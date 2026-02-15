using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Pool
{
    /// <summary>
    /// 泛型对象池基类，用于管理和复用对象，减少频繁创建和销毁带来的性能开销
    /// </summary>
    /// <typeparam name="T">池中对象的类型，必须是类且可被new()创建</typeparam>
    public abstract class DataObjectPool<T> : MonoBehaviour where T : class, new()
    {
        [Header("对象池设置")] [Tooltip("初始池大小")] [SerializeField]
        protected int initialPoolSize = 100; // 初始化时创建的对象数量

        [Tooltip("最大池大小")] [SerializeField] protected int maxPoolSize = 500; // 对象池能容纳的最大对象数量

        protected Queue<T> objectPool = new(); // 使用队列存储池中的对象，先进先出

        /// <summary>
        /// 在对象被激活时调用，用于初始化单例模式和对象池
        /// </summary>
        protected virtual void Awake()
        {
            InitializeSingleton(); // 初始化单例模式，确保全局只有一个实例
            InitPool(); // 初始化对象池，创建初始数量的对象
        }

        /// <summary>
        ///     初始化单例模式
        /// 子类必须实现此方法来确保单例的正确创建
        /// </summary>
        protected abstract void InitializeSingleton();

        /// <summary>
        ///     初始化对象池
        /// 根据initialPoolSize创建对象并放入池中
        /// </summary>
        protected virtual void InitPool()
        {
            for (var i = 0; i < initialPoolSize; i++)
            {
                var obj = CreateNewObject(); // 创建新对象
                objectPool.Enqueue(obj); // 将对象加入队列
            }
        }

        /// <summary>
        ///     创建新的对象
        /// 使用泛型约束确保T类型可以被new()创建
        /// </summary>
        protected virtual T CreateNewObject()
        {
            return new T(); // 创建T类型的新实例
        }

        /// <summary>
        ///     从对象池中获取对象
        /// 如果池中有可用对象则取出，否则创建新对象（未达到最大池大小时）
        /// </summary>
        public virtual T Get()
        {
            T obj;

            if (objectPool.Count > 0)
            {
                obj = objectPool.Dequeue(); // 从队列中取出对象
            }
            else
            {
                // 检查是否达到最大池大小
                if (objectPool.Count >= maxPoolSize)
                {
                    Debug.LogWarning($"{typeof(T).Name}对象池已达到最大大小 {maxPoolSize}，无法扩展");
                    return null; // 如果达到最大大小，返回null
                }

                obj = CreateNewObject(); // 创建新对象
            }

            return obj; // 返回获取的对象
        }

        /// <summary>
        ///     将对象返回到对象池
        /// 重置对象状态，并将其放回池中（未达到最大池大小时）
        /// </summary>
        public virtual void Return(T obj)
        {
            ResetObject(obj); // 重置对象状态

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