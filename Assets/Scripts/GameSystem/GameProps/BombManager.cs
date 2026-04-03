using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.EventSystem;
using GameSystem.GameScene.MainMenu.Map;
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
            if (MapInfo.Instance.HasTag(evt.Position, TagType.Bomb))
            {
                print("炸弹放置失败，位置有障碍物");
                evt.CallBack?.Invoke(false);
                return;
            }
            evt.Position.x = Mathf.Ceil(evt.Position.x) - 0.5f;
            evt.Position.z = Mathf.Ceil(evt.Position.z) - 0.5f;
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
            var bomb = BombPool.Instance.GetBomb();
            if (bomb != null)
            {
                bomb.gameObject.transform.position = evt.Position;
                bomb.gameObject.transform.rotation = Quaternion.identity;
                bomb.ownerId = evt.Id;
                bomb.putPosition = evt.Position;
                bomb.bombFuseTime = evt.BombFuseTime;
                bomb.bombRadius = evt.BombRadius;
                bomb.bombDamage = evt.BombDamage;
            }
            else
            {
                Debug.LogError("炸弹预制体上没有Bomb组件");
                evt.CallBack?.Invoke(false);
            }
            bomb.gameObject.SetActive(true);

            
            MapInfo.Instance.AddItem(evt.Position, bomb, TagType.Bomb);
            // 记录位置
            placedBombPositions.Add(bomb.putPosition);
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