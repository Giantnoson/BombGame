using System.Collections.Generic;
using GameSystem.Pool;
using UnityEngine;

namespace Game_props
{
    public class ExplodePool : ObjectPool<Explode>
    {
        public static ExplodePool Instance { get; private set; }

        private HashSet<Vector3> placedExplodePositions = new HashSet<Vector3>();

        protected override void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        protected override void ValidatePrefab()
        {
            // 重写以使用特定的变量名和错误消息
            if (prefab == null)
            {
                Debug.LogError("explodePrefab为空");
                return;
            }

            print("explodePrefab加载成功");
            Explode explode = prefab.GetComponent<Explode>();
            if (explode == null)
            {
                Debug.LogError("explodePrefab预制体上没有Explode组件");
            }
        }

        public void GetExplode(Vector3 position, Quaternion rotation)
        {
            if (placedExplodePositions.Contains(position))
            {
                Debug.Log("该位置已经存在爆炸效果，不创建新的爆炸效果");
                return;
            }

            GameObject explosion = GetObjectFromPool();
            explosion.transform.position = position;
            explosion.transform.rotation = rotation;
            explosion.SetActive(true);

            // 添加位置到已放置位置集合
            placedExplodePositions.Add(position);
        }

        public void ReturnExplode(GameObject explode)
        {
            // 从已放置位置集合中移除
            placedExplodePositions.Remove(explode.transform.position);
            ReturnObject(explode);
        }
    }
}
