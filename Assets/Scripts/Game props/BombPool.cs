using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game_props
{
    public class BombPool : MonoBehaviour
    {
        public static BombPool Instance { get; private set; }
        [Header("对象池设置")]
        [Tooltip("炸弹预制体")]
        public GameObject bombPrefab;
        [Tooltip("初始池大小")]
        public int initialPoolSize = 20;
        [Tooltip("池扩展大小")]
        public int poolExpandSize = 10;

        private Queue<GameObject> bombPool = new Queue<GameObject>();
        private Transform _poolContainer;

        private void Awake()
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
            
            if (bombPrefab == null)
            {
                Debug.LogError("bomb为空");
            }
            else
            {
                print("bomb加载成功");
                // 在运行时无法检查预制体状态，此检查已移至编辑器脚本

                Bomb bomb = bombPrefab.GetComponent<Bomb>();
                if (bomb == null)
                {
                    Debug.LogError("bomb预制体上没有Bomb组件"); 
                }
            }
            
            _poolContainer = new GameObject("BombPoolContainer").transform;
            _poolContainer.SetParent(transform);//将对象池容器设置为当前对象的子对象
            
            InitPool();
        }


        private void Start()
        {

        }

        private void InitPool()//初始化对象池
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
               GameObject bomb = Instantiate(bombPrefab, _poolContainer);
               bomb.SetActive(false);
               bombPool.Enqueue(bomb);
            }
        }

        private GameObject CreateNewBomb()//创建新的对象
        {
            GameObject bomb = Instantiate(bombPrefab, _poolContainer);
            return bomb;
        }

        /// <summary>
        /// 从对象池中获取炸弹对象
        /// 注意：需要手动启用炸弹对象
        /// </summary>
        /// <returns>返回一个炸弹游戏对象</returns>
        public GameObject GetBomb()//获取对象
        {
            GameObject bomb; // 用于存储获取到的炸弹对象

             // 检查对象池中是否有可用对象
            if (bombPool.Count > 0)
            {
                bomb = bombPool.Dequeue(); // 从对象池中取出一个炸弹对象
            }
            else
            {
                // 如果对象池为空，扩展对象池
                for (int i = 0; i < poolExpandSize - 1; i++)//扩展对象池少一次是因为最后还要再创建一个
                {
                    GameObject newBomb = CreateNewBomb(); // 创建新的炸弹对象
                    newBomb.SetActive(false); // 将新创建的炸弹设为非激活状态
                    bombPool.Enqueue(newBomb); // 将新创建的炸弹加入对象池
                }
                bomb = CreateNewBomb(); // 创建并返回一个新的炸弹对象
            }
            return bomb; // 返回获取或创建的炸弹对象
        }

        public void ReturnBomb(GameObject bomb) //返回对象
        {
            resetBomb(bomb);
            bomb.SetActive(false);
            bomb.transform.SetParent(_poolContainer);
            bombPool.Enqueue(bomb);
        }

        private void resetBomb(GameObject bomb)
        {
            Collider collider = bomb.GetComponent<Collider>();
            collider.enabled = true;
            collider.isTrigger = true;
        }
    }
}