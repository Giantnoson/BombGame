
using System;
using System.Collections.Generic;

namespace GameSystem.Map
{
    /// <summary>
    /// 最小堆泛型类，支持自定义比较器
    /// </summary>
    /// <typeparam name="T">堆中元素的类型</typeparam>
    public class MinHeap<T>
    {
        private const int InitialCapacity = 4; // 初始容量常量

        private List<T> _arr; // 存储堆元素的数组
        private int _lastItemIndex; // 最后一个元素的索引
        private readonly IComparer<T> _comparer; // 比较器

        /// <summary>
        /// 指定容量的构造函数，使用默认比较器
        /// </summary>
        /// <param name="capacity">堆的初始容量</param>
        public MinHeap()
            : this(Comparer<T>.Default)
        {
            
        }

        /// <summary>
        /// 使用比较函数的构造函数，使用初始容量
        /// </summary>
        /// <param name="comparison">比较函数</param>
        public MinHeap(Comparison<T> comparison)
            : this(Comparer<T>.Create(comparison))
        {
            
        }
        
        /// <summary>
        /// 完整构造函数，指定容量和比较器
        /// </summary>
        /// <param name="capacity">堆的初始容量</param>
        /// <param name="comparer">比较器</param>
        public MinHeap(IComparer<T> comparer)
        {
            _lastItemIndex = -1;
            _comparer = comparer;
        }

        /// <summary>
        /// 获取堆中元素的数量
        /// </summary>
        public int Count
        {
            get
            {
                return _lastItemIndex + 1;
            }
        }

        /// <summary>
        /// 向堆中添加元素
        /// </summary>
        /// <param name="item">要添加的元素</param>
        public void Add(T item)
        {
            _lastItemIndex++;
            _arr[_lastItemIndex] = item;

            MinHeapUp(_lastItemIndex);
        }

        /// <summary>
        /// 移除并返回堆中的最小元素
        /// </summary>
        /// <returns>堆中的最小元素</returns>
        public T Pop()
        {
            if (_lastItemIndex == -1)
            {
                throw new InvalidOperationException("The heap is empty");
            }

            T removedItem = _arr[0];
            _arr[0] = _arr[_lastItemIndex];
            _lastItemIndex--;

            MinHeapDown(0);

            return removedItem;
        }

        /// <summary>
        /// 获取堆中的最小元素但不移除
        /// </summary>
        /// <returns>堆中的最小元素</returns>
        public T Peek()
        {
            if (_lastItemIndex == -1)
            {
                throw new InvalidOperationException("The heap is empty");
            }

            return _arr[0];
        }

        /// <summary>
        /// 清空堆中的所有元素
        /// </summary>
        public void Clear()
        {
            _lastItemIndex = -1;
        }

        /// <summary>
        /// 检查堆中是否包含指定的元素
        /// </summary>
        /// <param name="item">堆元素</param>
        /// <returns>返回是否包含指定的元素的结果</returns>

        public bool Contains(T item)
        {
            return _arr.Contains(item);
        }

        /// <summary>
        /// 向上调整堆，保持堆性质
        /// </summary>
        /// <param name="index">开始调整的索引</param>
        private void MinHeapUp(int index)
        {
            int childIndex;
            int parentIndex;
            var flag = false;
            do
            {
                if (index == 0)
                {
                    return;
                }

                childIndex = index;
                parentIndex = (index - 1) / 2;

                if (_comparer.Compare(_arr[childIndex], _arr[parentIndex]) < 0)
                {
                    /*var temp = _arr[childIndex];
                    _arr[childIndex] = _arr[parentIndex];
                    _arr[parentIndex] = temp;*/
                    //元组交换，优于析构交换
                    (_arr[childIndex], _arr[parentIndex]) = (_arr[parentIndex], _arr[childIndex]);
                    index = parentIndex;
                    flag = true;
                }
            } while (flag);
        }

        private void MinHeapDown(int index)
        {

            int leftChildIndex;
            int rightChildIndex;
            int smallestItemIndex;
            var flag = false;
            do
            {
                leftChildIndex = index * 2 + 1;
                rightChildIndex = index * 2 + 2;
                smallestItemIndex = index;
                //
                if (leftChildIndex <= _lastItemIndex &&
                    _comparer.Compare(_arr[leftChildIndex], _arr[smallestItemIndex]) < 0)
                {
                    smallestItemIndex = leftChildIndex;
                }

                if (rightChildIndex <= _lastItemIndex &&
                    _comparer.Compare(_arr[rightChildIndex], _arr[smallestItemIndex]) < 0)
                {
                    smallestItemIndex = rightChildIndex;
                }
                if (smallestItemIndex != index)
                {
                    /*var temp = _arr[index];
                    _arr[index] = _arr[smallestItemIndex];
                    _arr[smallestItemIndex] = temp;*/
                    (_arr[index], _arr[smallestItemIndex]) = (_arr[smallestItemIndex], _arr[index]);

                    index = smallestItemIndex;
                    flag = true;
                }
            } while (flag);
        }
    }
}