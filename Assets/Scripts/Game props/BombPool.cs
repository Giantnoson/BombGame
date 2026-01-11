using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game_props
{
    public class BombPool : ObjectPool<Bomb>
    {
        public static BombPool Instance { get; private set; }

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
                Debug.LogError("bomb为空");
                return;
            }

            print("bomb加载成功");
            // 在运行时无法检查预制体状态，此检查已移至编辑器脚本

            Bomb bomb = prefab.GetComponent<Bomb>();
            if (bomb == null)
            {
                Debug.LogError("bomb预制体上没有Bomb组件");
            }
        }

        /// <summary>
        /// 从对象池中获取炸弹对象
        /// 注意：需要手动启用炸弹对象
        /// </summary>
        /// <returns>返回一个炸弹游戏对象</returns>
        public GameObject GetBomb()
        {
            return GetObjectFromPool();
        }

        public void ReturnBomb(GameObject bomb)
        {
            ReturnObject(bomb);
        }

        protected override void ResetObject(GameObject bomb)
        {
            Collider collider = bomb.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = true;
            }
        }
    }
}
