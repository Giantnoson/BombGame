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
            Debug.Log("BombManager启用，开始监听事件");
            EventSystem.AddListener<BombPlaceRequestEvent>(OnPlaceRequest);
            EventSystem.AddListener<BombDestroyEvent>(OnBombExplode);
        }

        private void OnDisable()
        {
            Debug.Log("BombManager销毁，停止监听事件");
            EventSystem.RemoveListener<BombPlaceRequestEvent>(OnPlaceRequest);
            EventSystem.RemoveListener<BombDestroyEvent>(OnBombExplode);
        }

        private void OnPlaceRequest(BombPlaceRequestEvent evt)
        {
            print("收到放置炸弹请求，位置：" + evt.position + "，炸弹预制体：" + evt.bombPrefab.name + "，拥有者：" + evt.ownerId);
            // 检查该位置是否已有炸弹
            if (placedBombPositions.Contains(evt.position))
            {
                Debug.Log("此处已有炸弹，无法放置");
                return;
            }
            
            // 放置炸弹
            GameObject bombInstance = Instantiate(evt.bombPrefab, evt.position, Quaternion.identity);
            
            Bomb bombComponent = bombInstance.GetComponent<Bomb>();
            if (bombComponent != null)
            {
                bombComponent.ownerId = evt.ownerId;
                bombComponent.putPosition = evt.position;
                bombComponent.bombFuseTime = evt.bombFuseTime;
                bombComponent.bombRadius = evt.bombRadius;
                bombComponent.bombDamage = evt.bombDamage;
            }
            else
            {
                Debug.LogError("炸弹预制体上没有Bomb组件");
            }
            
            // 记录位置
            placedBombPositions.Add(bombComponent.putPosition);

        }
        
        // 示例：移除位置的逻辑
        private void OnBombExplode(BombDestroyEvent evt)
        {
            
            print("收到炸弹爆炸事件，位置：" + evt.position + " , 炸弹拥有者：" + evt.ownerId);
            // TODO: 处理炸弹爆炸逻辑
            if (placedBombPositions.Contains(evt.position))
            {
                placedBombPositions.Remove(evt.position);
            }
            else
            {
                print("此处没有炸弹，无法移除");
            }
        }
    }
}
