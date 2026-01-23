using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Character.Enemy
{
    internal class Fsm<T> :FsmBase, IFsm<T> where T : class
    {
        public string Name
        {
            get { return _name; }
            protected set{ _name = value;}
            
        }//Fsm名称
        public T Owner
        {
            get { return _owner; }
        }//拥有者

        public override Type OwnerType
        {
            get { return typeof(T); }
        }

        public override int FsmStateCount
        {
            get { return _states.Count; }
        }//状态数量
        public override bool IsRunning
        {
            get { return _isRunning; }
        }//是否运行中
        public override bool IsDestroyed
        {
            get { return _isDestroyed; }
        }//是否销毁

        public override string CurrentStateName
        {
            get { return _currentState != null ? _currentState.GetType().ToString() : null; }
        }//当前状态名称} }

        public bool IsPaused
        {
            get { return _isPaused; }
        }//是否暂停
        public FsmState<T> CurrentState
        {
            get { return _currentState; }
        }//当前状态
        public override float CurrentStateTime
        {
            get { return _currentStateTime; }
        }//当前状态时间
        
        private string _name;
        private T _owner;
        private readonly Dictionary<Type, FsmState<T>> _states;
        private FsmState<T> _currentState;
        private float _currentStateTime;
        private bool _isRunning;
        private bool _isPaused;
        private bool _isDestroyed;
        
        public Fsm()
        {
            _owner = null;
            _isRunning = false;
            _isPaused = false;
            _isDestroyed = false;
            _currentState = null;
            _currentStateTime = 0f;
            _states = new Dictionary<Type, FsmState<T>>();
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
            if (owner == null)
            {
                Debug.LogError("FSM : Owner is invalid.");
            }
            if (states == null || states.Length <= 0)
            {
                Debug.LogError("FSM : States is invalid.");
            }
            Fsm<T> fsm = new Fsm<T>();
            fsm._name = name;
            fsm._owner = owner;
            fsm._isRunning = true;
            foreach (var state in states)
            {
                if (state == null)
                {
                    Debug.LogError("FSM : State is invalid.");
                }

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
            if (owner == null)
            {
                Debug.LogError("FSM : Owner is invalid.");
            }
            if (states == null || states.Count <= 0)
            {
                Debug.LogError("FSM : States is invalid.");
            }
            Fsm<T> fsm = new Fsm<T>();
            fsm._name = name;
            fsm._owner = owner;
            fsm._isRunning = true;
            foreach (var state in states)
            {
                if (state == null)
                {
                    Debug.LogError("FSM : State is invalid.");
                }

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
            if (_currentState != null)
            {
                _currentState.OnLeave(this, true);
            }

            foreach (KeyValuePair<Type, FsmState<T>> state in _states)
            {
                state.Value.OnDestroy(this);
            }

            Name = null;
            _owner = null;
            _states.Clear();
            _currentState = null;
            _currentStateTime = 0f;
            _isDestroyed = true;
        }
   

        public void Start<TState>() where TState : FsmState<T>
        {
            if (_isRunning)
            {
                if (_currentState != null)
                {
                    _currentState.OnLeave(this, false);
                }
                _currentState = GetState<TState>();
                _currentState.OnEnter(this);
            }
        }

        public void Start(Type stateType)
        {
            if (_isRunning)
            {
                if (_currentState != null)
                {
                    _currentState.OnLeave(this, false);
                }
                _currentState = GetState(stateType);
                _currentState.OnEnter(this);
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
            if (_states.TryGetValue(typeof(TState), out state))
            {
                return (TState)state;
            }
            else
            {
                return null;
            }
        }

        public FsmState<T> GetState(Type stateType)
        {
            FsmState<T> state = null;
            if (_states.TryGetValue(stateType, out state))
            {
                return state;
            }
            else
            {
                return null;
            }
        }

        public FsmState<T>[] GetAllStates()
        {
            int index = 0;
            FsmState<T>[] results = new FsmState<T>[_states.Count];
            foreach (var state in _states)
            {
                results[index++] = state.Value;
            }

            return results;
        }

        public void GetAllStates(List<FsmState<T>> results)
        {
            if (results == null)
            {
                Debug.LogError("FSM : Results is invalid.");
            }
            results.Clear();
            foreach (var state in _states)
            {
                results.Add(state.Value);
            }
        }

        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_currentState == null)
            {
                return;
            }

            _currentStateTime += elapseSeconds;
            _currentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
        }
        

        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        internal void ChangeState<TState>() where TState : FsmState<T>
        {
            ChangeState(typeof(TState));
        }
        
        /// <summary>
        /// 切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        internal void ChangeState(Type stateType)
        {
            if (_currentState == null)
            {
                Debug.LogError("Current state is invalid.");
                return;
            }

            FsmState<T> state = GetState(stateType);
            if (state == null)
            {
                Debug.LogError($"Can not find state '{stateType.Name}' from FSM '{Name}'.");
                return;
            }
            _currentState.OnLeave(this, false);
            _currentStateTime = 0f;
            _currentState = state;
            _currentState.OnEnter(this);
        }
        
        
        internal override void Shutdown()
        {
            
        }

    }
}