using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.EventSystem;
using GameSystem.GameScene.MainMenu.Map;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.GameProps
{
    public class BombPos : MonoBehaviour
    {
        private MapScan _mapScan;

        private Vector3[] drirect;
        public static BombPos Instance { get; private set; }

        /// <summary>
        ///     BombInfo用于记录炸弹的放置信息
        /// </summary>
        public Dictionary<Vector3, BombEvents.BombPlaceRequestEvent> BombInfo { get; } = new();

        public Dictionary<Vector3, int> BombExportArea { get; } = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            drirect = new[]
            {
                Vector3.back,
                Vector3.forward,
                Vector3.left,
                Vector3.right
            };
            _mapScan = FindAnyObjectByType<MapScan>();
            if (_mapScan == null) Debug.LogError("找不到MapScan");
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

        public Vector3 ToBombPutPos(Vector3 pos)
        {
            pos.x = Mathf.Ceil(pos.x) - 0.5f;
            pos.z = Mathf.Ceil(pos.z) - 0.5f;
            pos.y = 0f;
            return pos;
        }

        public bool IsInExportArea(Vector3 pos)
        {
            return BombExportArea.ContainsKey(ToBombPutPos(pos));
        }

        public bool IsInBombArea(Vector3 pos)
        {
            return BombInfo.ContainsKey(ToBombPutPos(pos));
        }


        private void BombInfoListener(BombEvents.BombPlaceRequestEvent evt)
        {
            evt.Position.x = Mathf.Ceil(evt.Position.x) - 0.5f;
            evt.Position.z = Mathf.Ceil(evt.Position.z) - 0.5f;
            evt.Position.y = 0f;
            if (!BombInfo.ContainsKey(evt.Position))
            {
                BombInfo.Add(evt.Position, evt);
                if (BombExportArea.ContainsKey(evt.Position))
                    BombExportArea[evt.Position]++;
                else
                    BombExportArea.Add(evt.Position, 1);
                Vector3 current;
                foreach (var dri in drirect) //记录炸弹爆炸范围
                {
                    current = evt.Position;
                    for (var i = 0; i < evt.BombRadius; i++)
                    {
                        current += dri;
                        if (_mapScan.CompareTag(current, ObjectType.Wall)) //当此处为墙时，返回
                            break;

                        if (BombExportArea.ContainsKey(current))
                            BombExportArea[current]++;
                        else
                            BombExportArea.Add(current, 1);
                    }
                }
            }
        }

        public void BombRemoveListener(BombEvents.BombDestroyEvent evt)
        {
            if (BombInfo.ContainsKey(evt.Position))
            {
                var bombInfo = BombInfo[evt.Position];
                if (BombExportArea.ContainsKey(bombInfo.Position))
                {
                    BombExportArea[bombInfo.Position]--;
                    if (BombExportArea[bombInfo.Position] == 0)
                        BombExportArea.Remove(bombInfo.Position);
                }

                Vector3 current;
                foreach (var dri in drirect)
                {
                    current = bombInfo.Position;
                    for (var i = 0; i < bombInfo.BombRadius; i++)
                    {
                        current += dri;
                        if (_mapScan.CompareTag(current, ObjectType.Wall))
                            break;
                        if (BombExportArea.ContainsKey(current))
                        {
                            BombExportArea[current]--;
                            if (BombExportArea[current] == 0)
                                BombExportArea.Remove(current);
                        }
                    }
                }

                BombInfo.Remove(evt.Position);
            }
            else
            {
                Debug.LogError("炸弹数量出现问题");
            }
        }
    }
}