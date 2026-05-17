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
        
        /// <summary>
        ///     待移除的计时器列表
        /// </summary>
        private readonly List<string> _timerToRemove = new();

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
            _timerToRemove.Clear();
            foreach (var timer in _timers)
            {
                timer.Value.Update();
                if (timer.Value.IsComplete) _timerToRemove.Add(timer.Key);
            }
            foreach (var key in _timerToRemove)
            {
                _timers.Remove(key);
            }
        }


        /// <summary>
        ///     创建计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <param name="expireTime">到期时间戳（基于 LocalTime.Now 的绝对时间，由调用方定义）</param>
        /// <param name="isLoop">是否为定时触发模式</param>
        /// <param name="useTimeScale">是否受 Time.timeScale 影响，默认 true（使用 Unity 游戏时间）</param>
        /// <param name="onEnable">开始时执行的函数</param>
        /// <param name="onComplete">完成时执行的函数</param>
        public void CreateTimer(string name, long expireTime,  Action onEnable = null, Action onComplete = null, bool isLoop = false, bool useTimeScale = true)
        {
            if (_timers.ContainsKey(name))
                Debug.LogWarning($"Timer {name} already exists.");
            else
                _timers.Add(name, new Timer(expireTime, isLoop, useTimeScale, onEnable, onComplete));
        }

        /// <summary>
        ///     创建或重置计时器（重复时触发旧 Timer 的 onComplete 回调以清理状态，再创建新 Timer）
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <param name="expireTime">到期时间戳（基于 LocalTime.Now 的绝对时间，由调用方定义）</param>
        /// <param name="onEnable">开始时执行的函数</param>
        /// <param name="onComplete">完成时执行的函数</param>
        /// <param name="isLoop">是否为定时触发模式</param>
        /// <param name="useTimeScale">是否受 Time.timeScale 影响，默认 true（使用 Unity 游戏时间）</param>
        public void CreateOrResetTimer(string name, long expireTime, Action onEnable = null, Action onComplete = null, bool isLoop = false, bool useTimeScale = true)
        {
            if (_timers.TryGetValue(name, out var existingTimer))
            {
                // 先触发旧 Timer 的 onComplete 回调，清理旧的道具状态
                // （如 PropsDisable → 广播 → activeProps.Remove + ApplyPropsEffect(false) + Destroy）
                existingTimer.ForceComplete();
                _timers.Remove(name);
            }
            _timers.Add(name, new Timer(expireTime, isLoop, useTimeScale, onEnable, onComplete));
        }
        
        /// <summary>
        ///     取消并移除指定计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <returns>是否成功取消</returns>
        public bool CancelTimer(string name)
        {
            return _timers.Remove(name);
        }
        
        /// <summary>
        ///     查询是否存在指定计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <returns>是否存在</returns>
        public bool HasTimer(string name)
        {
            return _timers.ContainsKey(name);
        }
    }


    public class Timer
    {
        private readonly Action _onComplete;

        /**
         * 完成回调
         */
        private readonly Action _onEnable;

        /// <summary>
        ///     到期时间戳（基于 LocalTime.Now，非 timeScale 模式使用）
        /// </summary>
        private long _expireTime;

        /// <summary>
        ///     暂停时记录的时间戳，0 表示未暂停
        /// </summary>
        private long _pauseStartTime;

        /// <summary>
        ///     受 timeScale 影响的累计时间（仅 timeScale 模式使用）
        /// </summary>
        private float _scaledElapsed;

        /// <summary>
        ///     是否受 Time.timeScale 影响
        /// </summary>
        private readonly bool _useTimeScale;


        //尾部为可选参数
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="expireTime">到期时间戳（基于 LocalTime.Now 的绝对时间，由调用方定义）</param>
        /// <param name="isLoop">是否为定时触发模式</param>
        /// <param name="useTimeScale">是否受 Time.timeScale 影响，默认 true（使用 Unity 游戏时间）</param>
        /// <param name="onEnable">开始时执行</param>
        /// <param name="onComplete">完成时执行</param>
        public Timer(long expireTime, bool isLoop = false, bool useTimeScale = true, Action onEnable = null, Action onComplete = null)
        {
            _onEnable = onEnable;
            _onComplete = onComplete;
            _useTimeScale = useTimeScale;
            _expireTime = expireTime;
            TotalTime = (float)Math.Max(0, (expireTime - LocalTime.Now) / 1000.0);
            _pauseStartTime = 0L;
            _scaledElapsed = 0f;
            IsComplete = false;
            IsLoop = isLoop;
            IsPause = false;
            _onEnable?.Invoke(); //如果存在onEnable，则调用
        }

        /**
         * 剩余时间（只读，根据模式计算）
         */
        public float RemainingTime
        {
            get
            {
                if (_useTimeScale)
                    return Mathf.Max(0, TotalTime - _scaledElapsed);
                if (IsPause)
                    return (float)Math.Max(0, (_expireTime - _pauseStartTime) / 1000.0);
                return (float)Math.Max(0, (_expireTime - LocalTime.Now) / 1000.0);
            }
        }

        /**
         * 记录总时间
         */
        public float TotalTime { get; }

        /**
         * 是否受 Time.timeScale 影响
         */
        public bool UseTimeScale => _useTimeScale;

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
        public void Update()
        {
            // 如果计时器暂停或已完成，则直接返回，不进行更新
            if (IsPause || IsComplete) return;

            if (_useTimeScale)
            {
                // timeScale 模式：累加受 timeScale 影响的 deltaTime
                _scaledElapsed += Time.deltaTime;
                if (_scaledElapsed >= TotalTime)
                {
                    IsComplete = true;
                    _onComplete?.Invoke();
                    if (IsLoop)
                    {
                        _scaledElapsed = 0f;
                        IsComplete = false;
                    }
                }
            }
            else
            {
                // 本地时间戳模式：检查当前时间是否已经到达或超过到期时间戳
                if (LocalTime.Now >= _expireTime)
                {
                    IsComplete = true;
                    _onComplete?.Invoke();
                    if (IsLoop)
                    {
                        _expireTime = LocalTime.Now + (long)(TotalTime * 1000);
                        IsComplete = false;
                    }
                }
            }
        }


        public void Pause()
        {
            if (IsPause) return;
            IsPause = true;
            if (!_useTimeScale)
                _pauseStartTime = LocalTime.Now;
        }

        public void Resume()
        {
            if (!IsPause) return;
            IsPause = false;
            if (!_useTimeScale)
            {
                // 将暂停期间的时间补偿到到期时间戳
                long pausedDuration = LocalTime.Now - _pauseStartTime;
                _expireTime += pausedDuration;
                _pauseStartTime = 0L;
            }
        }

        /// <summary>
        ///     强制触发完成回调（用于重置计时器时清理旧状态）
        ///     标记为完成但不触发循环重置逻辑
        /// </summary>
        public void ForceComplete()
        {
            if (IsComplete) return;
            IsComplete = true;
            _onComplete?.Invoke();
        }
    }
}
