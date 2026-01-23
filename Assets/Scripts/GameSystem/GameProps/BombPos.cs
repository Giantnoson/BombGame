using System;
using System.Collections;
using System.Collections.Generic;
using GameSystem.EventSystem;
using GameSystem.Map;
using UnityEngine;

namespace GameSystem.GameProps
{
    public class BombPos : MonoBehaviour
    {
        public static BombPos Instance { get; private set; }
        
        /// <summary>
        /// BombInfo用于记录炸弹的放置信息
        /// </summary>
        private Dictionary<Vector3,BombEvents.BombPlaceRequestEvent> _bombInfo = new Dictionary<Vector3, BombEvents.BombPlaceRequestEvent>();

        public Dictionary<Vector3, BombEvents.BombPlaceRequestEvent> BombInfo => _bombInfo;
        
        private Dictionary<Vector3, int> _bombExportArea = new Dictionary<Vector3, int>();
        
        public Dictionary<Vector3, int> BombExportArea => _bombExportArea;

        private Vector3[] drirect;
        
        private MapScan _mapScan;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            drirect = new Vector3[]
            {
                Vector3.back,
                Vector3.forward,
                Vector3.left,
                Vector3.right
            };
            _mapScan = FindAnyObjectByType<MapScan>();
            if (_mapScan == null)
            {
                Debug.LogError("找不到MapScan");
            }
        }
        
        private void OnEnable()
        {
            GameEventSystem.AddListener<BombEvents.BombPlaceRequestEvent>(BombInfoListener);
            GameEventSystem.AddListener<BombEvents.BombDestroyEvent>(BombRemoveListener);
        }

        private void OnDisable()
        {
            GameEventSystem.RemoveListener<BombEvents.BombPlaceRequestEvent>(BombInfoListener);
            GameEventSystem.RemoveListener<BombEvents.BombDestroyEvent>(BombRemoveListener);
        }

        private void BombInfoListener(BombEvents.BombPlaceRequestEvent evt)
        {
            if (!_bombInfo.ContainsKey(evt.Position))
            {
                _bombInfo.Add(evt.Position, evt);
                if (_bombExportArea.ContainsKey(evt.Position))
                {
                    _bombExportArea[evt.Position]++;
                }
                else
                {
                    _bombExportArea.Add(evt.Position, 1);
                }
                Vector3 current;
                foreach (var dri in drirect)//记录炸弹爆炸范围
                {
                    current = evt.Position;
                    for (int i = 0; i < evt.BombRadius; i++)
                    {
                        current += dri;
                        if (_mapScan.CompareTag(current, ObjectType.Wall))//当此处为墙时，返回
                        {
                            break;
                        }

                        if (_bombExportArea.ContainsKey(current))
                        {
                            _bombExportArea[current]++;
                        }
                        else
                        {
                            _bombExportArea.Add(current, 1);
                        }

                    }
                }
            }
                
        }

        public void BombRemoveListener(BombEvents.BombDestroyEvent evt)
        {


            if (_bombInfo.ContainsKey(evt.Position))
            {
                var bombInfo = _bombInfo[evt.Position];
                if (_bombExportArea.ContainsKey(bombInfo.Position))
                {
                    _bombExportArea[bombInfo.Position]--;
                    if (_bombExportArea[bombInfo.Position] == 0)
                        _bombExportArea.Remove(bombInfo.Position);
                }

                Vector3 current;
                foreach (var dri in drirect)
                {
                    current = bombInfo.Position;
                    for (int i = 0; i < bombInfo.BombRadius; i++)
                    {
                        current += dri;
                        if (_mapScan.CompareTag(current, ObjectType.Wall))
                            break;
                        if (_bombExportArea.ContainsKey(current))
                        {
                            _bombExportArea[current]--;
                            if (_bombExportArea[current] == 0)
                                _bombExportArea.Remove(current);
                        }
                    }
                }
                _bombInfo.Remove(evt.Position);
            }
            else
            {
                Debug.LogError("炸弹数量出现问题");
            }
        }
    }
}