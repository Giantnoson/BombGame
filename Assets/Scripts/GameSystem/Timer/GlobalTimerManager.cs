using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Timer
{
    public class GlobalTimerManager : MonoBehaviour
    {
        /// <summary>
        ///     计时器字典
        /// </summary>
        private readonly Dictionary<string, Timer> _timers = new();

        //单例模式
        public static GlobalTimerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            foreach (var timer in _timers)
            {
                timer.Value.Update(Time.deltaTime);
                if (timer.Value.IsComplete) _timers.Remove(timer.Key);
            }
        }


        /// <summary>
        ///     创建计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <param name="totalTime">最大时间</param>
        /// <param name="isLoop">是否为定时触发模式</param>
        /// <param name="onEnable">开始时执行的函数</param>
        /// <param name="onComplete">完成时执行的函数</param>
        public void CreateTimer(string name, float totalTime, bool isLoop = false, Action onEnable = null, Action onComplete = null)
        {
            if (_timers.ContainsKey(name))
                Debug.LogWarning($"Timer {name} already exists.");
            else
                _timers.Add(name, new Timer(totalTime, isLoop, onEnable, onComplete));
        }
    }


    public class Timer
    {
        private readonly Action _onComplete;

        /**
         * 完成回调
         */
        private readonly Action _onEnable;


        //尾部为可选参数
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="totalTime">最大时间</param>
        /// <param name="isLoop">是否为定时触发模式</param>
        /// <param name="onEnable">开始时执行</param>
        /// <param name="onComplete">完成时执行</param>
        public Timer(float totalTime, bool isLoop = false, Action onEnable = null, Action onComplete = null)
        {
            _onEnable = onEnable;
            _onComplete = onComplete;
            CurrentTime = totalTime;
            TotalTime = totalTime;
            IsComplete = false;
            IsLoop = isLoop;
            IsPause = false;
            _onEnable?.Invoke(); //如果存在onEnable，则调用
        }

        /**
         * 记录当前时间
         */
        public float CurrentTime { get; private set; }

        /**
         * 记录总时间
         */
        public float TotalTime { get; }

        /**
         * 是否完成
         */
        public bool IsComplete { get; private set; }

        /**
         * 是否循环
         */
        public bool IsLoop { get; }

        /**
         * 是否暂停
         */
        public bool IsPause { get; private set; }
        
        /// <summary>
        ///     更新计时器状态的方法
        /// </summary>
        /// <param name="deltaTime">距离上一帧的时间间隔</param>
        public void Update(float deltaTime)
        {
            // 如果计时器暂停或已完成，则直接返回，不进行更新
            if (IsPause || IsComplete) return;
            // 减少当前计时器的时间
            CurrentTime -= deltaTime;
            // 检查计时器是否已经到达或超过设定时间
            if (CurrentTime <= 0)
            {
                // 确保当前时间不会小于0
                CurrentTime = 0;
                // 标记计时器为已完成状态
                IsComplete = true;
                // 触发完成事件（如果有订阅者）
                _onComplete?.Invoke();
                // 如果设置为循环模式
                if (IsLoop)
                {
                    // 重置当前时间为总时间，实现循环效果
                    CurrentTime = TotalTime;
                    // 重置完成状态，以便继续下一次计时
                    IsComplete = false;
                }
            }
        }


        public void Pause()
        {
            IsPause = true;
        }

        public void Resume()
        {
            IsPause = false;
        }
    }
}