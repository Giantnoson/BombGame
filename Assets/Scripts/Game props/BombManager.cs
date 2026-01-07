using System.Collections.Generic;
using UnityEngine;
using GameSystem;

namespace Game_props
{
    public class BombManager : MonoBehaviour
    {
        private HashSet<Vector3> placedBombPositions = new HashSet<Vector3>();

        private void OnEnable()
        {
            Debug.Log("BombManager Enabled and listening for events");
            EventSystem.AddListener<BombPlaceRequestEvent>(OnPlaceRequest);
            EventSystem.AddListener<BombExplodeEvent>(OnBombExplode);
        }

        private void OnDisable()
        {
            Debug.Log("BombManager Disabled and stopped listening for events");
            EventSystem.RemoveListener<BombPlaceRequestEvent>(OnPlaceRequest);
            EventSystem.RemoveListener<BombExplodeEvent>(OnBombExplode);
        }

        private void OnPlaceRequest(BombPlaceRequestEvent evt)
        {
            print("收到放置炸弹请求，位置：" + evt.position + "，炸弹预制体：" + evt.bombPrefab.name);
            // 检查该位置是否已有炸弹
            if (placedBombPositions.Contains(evt.position))
            {
                Debug.Log("此处已有炸弹，无法放置");
                return;
            }
            
            // 放置炸弹
            Instantiate(evt.bombPrefab, evt.position, Quaternion.identity);
            // 记录位置
            placedBombPositions.Add(evt.position);

        }
        
        // 示例：移除位置的逻辑
        private void OnBombExplode(BombExplodeEvent evt)
        {
            if (placedBombPositions.Contains(evt.position))
            {
                placedBombPositions.Remove(evt.position);
            }
        }
    }
}
