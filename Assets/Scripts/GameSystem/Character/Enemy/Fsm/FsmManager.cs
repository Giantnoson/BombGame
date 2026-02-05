using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Enemy
{
    public class FsmManager : IFsmManager
    {
        private readonly Dictionary<Type, FsmBase> _fsm;
        private readonly List<FsmBase> _tmpFsms;

        public FsmManager()
        {
            _fsm = new Dictionary<Type, FsmBase>();
            _tmpFsms = new List<FsmBase>();
        }

        public int Count => _fsm.Count;

        public bool HasFsm<T>() where T : class
        {
            return InternalHasFsm(typeof(T));
        }

        public bool HasFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                Debug.LogError("Owner modeType is invalid.");
                return false;
            }

            return InternalHasFsm(ownerType);
        }

        public bool HasFsm<T>(string name) where T : class
        {
            return InternalHasFsm(typeof(T), name);
        }

        public bool HasFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                Debug.LogError("Owner modeType is invalid.");
                return false;
            }

            return InternalHasFsm(ownerType, name);
        }

        public IFsm<T> GetFsm<T>() where T : class
        {
            return (IFsm<T>)InternalGetFsm(typeof(T));
        }

        public FsmBase GetFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                Debug.LogError("Owner modeType is invalid.");
                return null;
            }

            return InternalGetFsm(ownerType);
        }

        public IFsm<T> GetFsm<T>(string name) where T : class
        {
            return (IFsm<T>)InternalGetFsm(typeof(T), name);
        }

        public FsmBase GetFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                Debug.LogError("Owner modeType is invalid.");
                return null;
            }

            return InternalGetFsm(ownerType, name);
        }

        public FsmBase[] GetAllFsms()
        {
            var index = 0;
            var results = new FsmBase[_fsm.Count];
            foreach (var fsm in _fsm) results[index++] = fsm.Value;

            return results;
        }

        public void GetAllFsms(List<FsmBase> results)
        {
            if (results == null)
            {
                Debug.LogError("Results is invalid.");
                return;
            }

            results.Clear();
            foreach (var fsm in _fsm) results.Add(fsm.Value);
        }

        public IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
        {
            var ownerType = typeof(T);
            if (HasFsm<T>(name)) Debug.LogError(string.Format("Already exist FSM '{0}'.", name));

            var fsm = Fsm<T>.Create(name, owner, states);
            _fsm.Add(ownerType, fsm);
            return fsm;
        }

        public IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
        {
            var ownerType = typeof(T);
            if (HasFsm<T>(name)) Debug.LogError(string.Format("Already exist FSM '{0}'.", name));

            var fsm = Fsm<T>.Create(name, owner, states);
            _fsm.Add(ownerType, fsm);
            return fsm;
        }

        public bool DestroyFsm<T>() where T : class
        {
            return InternalDestroyFsm(typeof(T));
        }

        public bool DestroyFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                Debug.LogError("Owner modeType is invalid.");
                return false;
            }

            return InternalDestroyFsm(ownerType);
        }

        public bool DestroyFsm<T>(string name) where T : class
        {
            return InternalDestroyFsm(typeof(T), name);
        }

        public bool DestroyFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                Debug.LogError("Owner modeType is invalid.");
                return false;
            }

            return InternalDestroyFsm(ownerType, name);
        }

        public bool DestroyFsm<T>(IFsm<T> fsm) where T : class
        {
            if (fsm == null)
            {
                Debug.LogError("FSM is invalid.");
                return false;
            }

            return InternalDestroyFsm(typeof(T), fsm.Name);
        }

        public bool DestroyFsm(FsmBase fsm)
        {
            if (fsm == null)
            {
                Debug.LogError("FSM is invalid.");
                return false;
            }

            return InternalDestroyFsm(fsm.OwnerType);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            _tmpFsms.Clear();
            if (_fsm.Count <= 0) return;

            foreach (var fsm in _fsm) _tmpFsms.Add(fsm.Value);

            foreach (var fsm in _tmpFsms)
            {
                if (fsm.IsDestroyed) continue;

                fsm.Update(elapseSeconds, realElapseSeconds);
            }
        }

        internal void Shutdown()
        {
            _fsm.Clear();
            _tmpFsms.Clear();
        }

        private bool InternalHasFsm(Type ownerType)
        {
            return _fsm.ContainsKey(ownerType);
        }

        private bool InternalHasFsm(Type ownerType, string name)
        {
            foreach (var fsm in _fsm)
                if (fsm.Key == ownerType && fsm.Value.Name == name)
                    return true;

            return false;
        }

        private FsmBase InternalGetFsm(Type ownerType)
        {
            FsmBase fsm = null;
            if (_fsm.TryGetValue(ownerType, out fsm)) return fsm;

            return null;
        }

        private FsmBase InternalGetFsm(Type ownerType, string name)
        {
            foreach (var fsm in _fsm)
                if (fsm.Key == ownerType && fsm.Value.Name == name)
                    return fsm.Value;

            return null;
        }

        private bool InternalDestroyFsm(Type ownerType)
        {
            FsmBase fsm = null;
            if (_fsm.TryGetValue(ownerType, out fsm))
            {
                fsm.Shutdown();
                return _fsm.Remove(ownerType);
            }

            return false;
        }

        private bool InternalDestroyFsm(Type ownerType, string name)
        {
            foreach (var fsm in _fsm)
                if (fsm.Key == ownerType && fsm.Value.Name == name)
                {
                    fsm.Value.Shutdown();
                    return _fsm.Remove(fsm.Key);
                }

            return false;
        }
    }
}