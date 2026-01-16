using System;
using System.Collections;
using System.Collections.Generic;
using Game_props;
using GameSystem.EventSystem;
using UnityEngine;

namespace Game_props
{
    public class BombPos : MonoBehaviour
    {
        public static BombPos Instance { get; private set; }
        
        
        private Dictionary<Vector3,BombPlaceRequestEvent> _bombInfo = new Dictionary<Vector3, BombPlaceRequestEvent>();

        public Dictionary<Vector3, BombPlaceRequestEvent> BombInfo => _bombInfo;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
        }
        
        private void OnEnable()
        {
            GameEventSystem.AddListener<BombPlaceRequestEvent>(BombInfoListener);
            GameEventSystem.AddListener<BombDestroyEvent>(BombRemoveListener);
        }

        private void OnDisable()
        {
            GameEventSystem.RemoveListener<BombPlaceRequestEvent>(BombInfoListener);
            GameEventSystem.RemoveListener<BombDestroyEvent>(BombRemoveListener);
        }

        private void BombInfoListener(BombPlaceRequestEvent evt)
        {
            if (!_bombInfo.ContainsKey(evt.Position))
                _bombInfo.Add(evt.Position, evt);
        }

        public void BombRemoveListener(BombDestroyEvent evt)
        { 
            _bombInfo.Remove(evt.Position);
        }
    }
}