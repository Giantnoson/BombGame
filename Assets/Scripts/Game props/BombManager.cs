using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using GameSystem.EventSystem;

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
            print("收到放置炸弹请求，位置：" + evt.Position + "，拥有者：" + evt.OwnerId);
            // 检查该位置是否已有炸弹
            if (placedBombPositions.Contains(evt.Position))
            {
                Debug.Log("此处已有炸弹，无法放置");
                return;
            }
            
            // 放置炸弹
            GameObject bombInstance = BombPool.Instance.GetBomb();
            bombInstance.gameObject.transform.position = evt.Position;
            bombInstance.gameObject.transform.rotation = Quaternion.identity;
            
            
            
            Bomb bombComponent = bombInstance.GetComponent<Bomb>();
            if (bombComponent != null)
            {
                bombComponent.ownerId = evt.OwnerId;
                bombComponent.putPosition = evt.Position;
                bombComponent.bombFuseTime = evt.BombFuseTime;
                bombComponent.bombRadius = evt.BombRadius;
                bombComponent.bombDamage = evt.BombDamage;
            }
            else
            {
                Debug.LogError("炸弹预制体上没有Bomb组件");
            }
            bombInstance.SetActive(true);
            // 记录位置
            placedBombPositions.Add(bombComponent.putPosition);

        }
        
        // 示例：移除位置的逻辑
        private void OnBombExplode(BombDestroyEvent evt)
        {
            
            print("收到炸弹爆炸事件，位置：" + evt.Position + " , 炸弹拥有者：" + evt.OwnerId);
            // TODO: 处理炸弹爆炸逻辑
            if (placedBombPositions.Contains(evt.Position))
            {
                placedBombPositions.Remove(evt.Position);
            }
            else
            {
                print("此处没有炸弹，无法移除");
                return;
            }
        }
    }
}
