using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Character.Enemy.Fsm
{
    internal class Fsm<T> : FsmBase, IFsm<T> where T : class
    {
        private readonly Dictionary<Type, FsmState<T>> _states;
        private float _currentStateTime;
        private bool _isDestroyed;
        private bool _isRunning;

        public Fsm()
        {
            Owner = null;
            _isRunning = false;
            IsPaused = false;
            _isDestroyed = false;
            CurrentState = null;
            _currentStateTime = 0f;
            _states = new Dictionary<Type, FsmState<T>>();
        }

        public override Type OwnerType => typeof(T);

        public override string CurrentStateName =>
            CurrentState != null ? CurrentState.GetType().ToString() : null; //当前状态名称} }

        public new string Name { get; protected set; } //Fsm名称

        public T Owner { get; private set; }

        public override int FsmStateCount => _states.Count; //状态数量
        public override bool IsRunning => _isRunning; //是否运行中
        public override bool IsDestroyed => _isDestroyed; //是否销毁

        public bool IsPaused { get; }

        public FsmState<T> CurrentState { get; private set; }

        public override float CurrentStateTime => _currentStateTime; //当前状态时间


        public void Start<TState>() where TState : FsmState<T>
        {
            if (_isRunning)
            {
                if (CurrentState != null) CurrentState.OnLeave(this, false);
                CurrentState = GetState<TState>();
                CurrentState.OnEnter(this);
            }
        }

        public void Start(Type stateType)
        {
            if (_isRunning)
            {
                if (CurrentState != null) CurrentState.OnLeave(this, false);
                CurrentState = GetState(stateType);
                CurrentState.OnEnter(this);
            }
        }

        public bool HasState<TState>() where TState : FsmState<T>
        {
            return _states.ContainsKey(typeof(TState));
        }

        public bool HasState(Type stateType)
        {
            return _states.ContainsKey(stateType);
        }

        public TState GetState<TState>() where TState : FsmState<T>
        {
            FsmState<T> state = null;
            if (_states.TryGetValue(typeof(TState), out state)) return (TState)state;

            return null;
        }

        public FsmState<T> GetState(Type stateType)
        {
            FsmState<T> state = null;
            if (_states.TryGetValue(stateType, out state)) return state;

            return null;
        }

        public FsmState<T>[] GetAllStates()
        {
            var index = 0;
            var results = new FsmState<T>[_states.Count];
            foreach (var state in _states) results[index++] = state.Value;

            return results;
        }

        public void GetAllStates(List<FsmState<T>> results)
        {
            if (results == null) Debug.LogError("FSM : Results is invalid.");
            results.Clear();
            foreach (var state in _states) results.Add(state.Value);
        }

        /// <summary>
        /// 创建一个有限状态机(FSM)实例
        /// </summary>
        /// <typeparam name="T">有限状态机所有者的类型</typeparam>
        /// <param name="name">有限状态机的名称</param>
        /// <param name="owner">有限状态机的所有者</param>
        /// <param name="states">有限状态机的状态集合</param>
        /// <returns>创建的有限状态机实例</returns>
        public static Fsm<T> Create(string name, T owner, params FsmState<T>[] states)
        {
            if (owner == null) Debug.LogError("FSM : Owner is invalid.");
            if (states == null || states.Length <= 0) Debug.LogError("FSM : States is invalid.");
            var fsm = new Fsm<T>();
            fsm.Name = name;
            fsm.Owner = owner;
            fsm._isRunning = true;
            foreach (var state in states)
            {
                if (state == null) Debug.LogError("FSM : State is invalid.");

                if (fsm._states.ContainsKey(state.GetType()))
                {
                    Debug.LogError("FSM : State already exists.");
                }
                else
                {
                    fsm._states.Add(state.GetType(), state);
                    state.OnInit(fsm);
                }
            }

            return fsm;
        }

        public static Fsm<T> Create(string name, T owner, List<FsmState<T>> states)
        {
            if (owner == null) Debug.LogError("FSM : Owner is invalid.");
            if (states == null || states.Count <= 0) Debug.LogError("FSM : States is invalid.");
            var fsm = new Fsm<T>();
            fsm.Name = name;
            fsm.Owner = owner;
            fsm._isRunning = true;
            foreach (var state in states)
            {
                if (state == null) Debug.LogError("FSM : State is invalid.");

                if (fsm._states.ContainsKey(state.GetType()))
                {
                    Debug.LogError("FSM : State already exists.");
                }
                else
                {
                    fsm._states.Add(state.GetType(), state);
                    state.OnInit(fsm);
                }
            }

            return fsm;
        }

        /// <summary>
        /// 清理有限状态机。
        /// </summary>
        public void Clear()
        {
            if (CurrentState != null) CurrentState.OnLeave(this, true);

            foreach (var state in _states) state.Value.OnDestroy(this);

            Name = null;
            Owner = null;
            _states.Clear();
            CurrentState = null;
            _currentStateTime = 0f;
            _isDestroyed = true;
        }

        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (CurrentState == null) return;

            _currentStateTime += elapseSeconds;
            CurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
        }


        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        internal void ChangeState<TState>() where TState : FsmState<T>
        {
            ChangeState(typeof(TState));
        }

        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        internal void ChangeState(Type stateType)
        {
            if (CurrentState == null)
            {
                Debug.LogError("Current state is invalid.");
                return;
            }

            var state = GetState(stateType);
            if (state == null)
            {
                Debug.LogError($"Can not find state '{stateType.Name}' from FSM '{Name}'.");
                return;
            }

            CurrentState.OnLeave(this, false);
            _currentStateTime = 0f;
            CurrentState = state;
            CurrentState.OnEnter(this);
        }


        internal override void Shutdown()
        {
        }
    }
}