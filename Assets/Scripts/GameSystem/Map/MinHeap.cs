using System;
using System.Collections.Generic;
using System.Linq;
 
namespace GameSystem.Map 
{
    /**
     * 泛型最小堆（MinHeap<T>）类实现
     * 最小堆是一种特殊的完全二叉树，其中每个父节点的值都小于或等于其子节点的值
     * @param <T> 堆中元素类型，必须实现IComparer<T>接口
     */
    public class MinHeap<T>
    {
        // 堆的初始容量
        private const int InitialCapacity = 4;
        // 用于存储堆元素的动态数组
        private List<T> _arr;
        // 指向堆中最后一个元素的索引
        private int _lastItemIndex;
        // 用于比较元素的比较器
        private readonly IComparer<T> _comparer;
 
        /**
         * 默认构造函数，使用默认的比较器
         */
        public MinHeap() : this(Comparer<T>.Default) { }
        
        /**
         * 使用自定义比较方法构造堆
         * @param comparison 自定义比较方法
         */
        public MinHeap(Comparison<T> comparison) 
            : this(Comparer<T>.Create(comparison)) { }
        
        /**
         * 使用自定义比较器构造堆
         * @param comparer 自定义比较器，如果为null则使用默认比较器
         */
        public MinHeap(IComparer<T> comparer) {
            _arr = new List<T>(InitialCapacity);
            _lastItemIndex = -1; // 初始化为-1表示堆为空
            _comparer = comparer ?? Comparer<T>.Default;
        }
 
        /**
         * 获取堆中元素的数量
         */
        public int Count => _lastItemIndex + 1;
 
        /**
         * 向堆中添加一个元素
         * @param item 要添加的元素
         */
        public void Add(T item) {
            _lastItemIndex++;
            // 如果数组还有空间，直接赋值；否则添加新元素
            if (_lastItemIndex < _arr.Count) {
                _arr[_lastItemIndex] = item;
            } else {
                _arr.Add(item);
            }
            // 上浮操作以维护堆性质
            MinHeapUp(_lastItemIndex);
        }
 
        /**
         * 移除并返回堆中的最小元素
         * @return 堆中的最小元素
         * @throws InvalidOperationException 如果堆为空
         */
        public T Pop() {
            if (_lastItemIndex == -1) throw new InvalidOperationException("Heap is empty");
            
            T removed = _arr[0]; // 保存要返回的根节点
            _arr[0] = _arr[_lastItemIndex]; // 将最后一个元素移到根节点
            _lastItemIndex--;
            
            // 如果堆不为空，进行下沉操作
            if (_lastItemIndex >= 0) MinHeapDown(0);
            return removed;
        }
 
        /**
         * 返回堆中的最小元素但不移除它
         * @return 堆中的最小元素
         * @throws InvalidOperationException 如果堆为空
         */
        public T Peek() => _lastItemIndex == -1 
            ? throw new InvalidOperationException("Heap is empty") 
            : _arr[0];
 
        /**
         * 清空堆
         */
        public void Clear() {
            _lastItemIndex = -1;
            _arr.Clear();
            _arr.TrimExcess(); // 释放多余空间
        }
 
        /**
         * 检查堆中是否包含指定元素
         * @param item 要查找的元素
         * @return 如果包含则返回true，否则返回false
         */
        public bool Contains(T item) {
            for (int i = 0; i <= _lastItemIndex; i++) {
                if (EqualityComparer<T>.Default.Equals(_arr[i], item))
                    return true;
            }
            return false;
        }
 
        /**
         * 上浮操作，用于维护堆性质
         * @param index 当前需要上浮的元素索引
         */
        private void MinHeapUp(int index) {
            while (index > 0) {
                int parent = (index - 1) / 2; // 父节点索引
            // 如果当前节点小于父节点，交换它们
                if (_comparer.Compare(_arr[index], _arr[parent]) < 0) {
                    (_arr[index], _arr[parent]) = (_arr[parent], _arr[index]);
                    index = parent;
                } else break; // 如果已经满足堆性质，退出循环
            }
        }
 
        /**
         * 下沉操作，用于维护堆性质
         * @param index 当前需要下沉的元素索引
         */
        ///
        private void MinHeapDown(int index) {
            while (true) {
                int left = 2 * index + 1; // 左子节点索引
                int right = 2 * index + 2; // 右子节点索引
                int smallest = index; // 当前最小元素的索引
 
                // 比较左子节点
                if (left <= _lastItemIndex && 
                    _comparer.Compare(_arr[left], _arr[smallest]) < 0) 
                    smallest = left;
                
                // 比较右子节点
                if (right <= _lastItemIndex && 
                    _comparer.Compare(_arr[right], _arr[smallest]) < 0) 
                    smallest = right;
                
                // 如果当前节点已经是最小，退出循环
                if (smallest == index) break;
                
                // 交换当前节点与最小子节点
                (_arr[index], _arr[smallest]) = (_arr[smallest], _arr[index]);
                index = smallest;
            }
        }
    }
}