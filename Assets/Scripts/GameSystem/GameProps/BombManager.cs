using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.EventSystem;
using GameSystem.GameScene.MainMenu.Pool;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.GameProps
{
    public class BombManager : MonoBehaviour
    {
        private readonly HashSet<Vector3> placedBombPositions = new();

        private Collider[] hitColliders = new Collider[20];
        
        
        private void OnEnable()
        {
            Debug.Log("BombManager启用，开始监听事件");
            GameEventSystem.AddListener<BombEvents.BombPlaceRequestEvent>(OnPlaceRequest);
            GameEventSystem.AddListener<BombEvents.BombDestroyEvent>(OnBombExplode);
        }

        private void OnDisable()
        {
            Debug.Log("BombManager销毁，停止监听事件");
            GameEventSystem.RemoveListener<BombEvents.BombPlaceRequestEvent>(OnPlaceRequest);
            GameEventSystem.RemoveListener<BombEvents.BombDestroyEvent>(OnBombExplode);
        }

        private void OnPlaceRequest(BombEvents.BombPlaceRequestEvent evt)
        {
            evt.Position.x = Mathf.Ceil(evt.Position.x) - 0.5f;
            evt.Position.z = Mathf.Ceil(evt.Position.z) - 0.5f;
            evt.Position.y = 0.5f;
            var len = Physics.OverlapBoxNonAlloc(evt.Position, new Vector3(0.4f, 0.4f, 0.4f), hitColliders);
            if (len > 0)
                for (int i = 0; i < len; i++)
                {
                    if (hitColliders[i].gameObject.CompareTag(nameof(ObjectType.Wall)))
                    {
                        print("炸弹放置失败，位置有障碍物");
                        return;
                    }
                }
            evt.Position.y = 0f;
            print("收到放置炸弹请求，位置：" + evt.Position + "，拥有者：" + evt.Id);
            // 检查该位置是否已有炸弹
            if (placedBombPositions.Contains(evt.Position))
            {
                Debug.Log("此处已有炸弹，无法放置");
                evt.CallBack?.Invoke(false);
                return;
            }

            // 放置炸弹
            var bombInstance = BombPool.Instance.GetBomb();
            bombInstance.gameObject.transform.position = evt.Position;
            bombInstance.gameObject.transform.rotation = Quaternion.identity;
            
            var bombComponent = bombInstance.GetComponent<Bomb>();
            if (bombComponent != null)
            {
                bombComponent.ownerId = evt.Id;
                bombComponent.putPosition = evt.Position;
                bombComponent.bombFuseTime = evt.BombFuseTime;
                bombComponent.bombRadius = evt.BombRadius;
                bombComponent.bombDamage = evt.BombDamage;
            }
            else
            {
                Debug.LogError("炸弹预制体上没有Bomb组件");
                evt.CallBack?.Invoke(false);
            }
            bombInstance.SetActive(true);
            // 记录位置
            placedBombPositions.Add(bombComponent.putPosition);
            evt.CallBack?.Invoke(true);
        }

        // 示例：移除位置的逻辑
        private void OnBombExplode(BombEvents.BombDestroyEvent evt)
        {
            print("收到炸弹爆炸事件，位置：" + evt.Position + " , 炸弹拥有者：" + evt.Id);
            // TODO: 处理炸弹爆炸逻辑
            if (placedBombPositions.Contains(evt.Position))
                placedBombPositions.Remove(evt.Position);
            else
                print("此处没有炸弹，无法移除");
        }
    }
}