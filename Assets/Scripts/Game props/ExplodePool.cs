using System.Collections.Generic;
using UnityEngine;

namespace Game_props
{
    public class ExplodePool : MonoBehaviour
    {
        public static ExplodePool Instance { get; private set; }

        [Header("对象池设置")]
        [Tooltip("爆炸效果预制体")]
        public GameObject explodePrefab;
        [Tooltip("初始池大小")]
        public int initialPoolSize = 40;
        [Tooltip("池扩展大小")]
        public int poolExpandSize = 20;
        
        private Queue<GameObject> _explodePool = new Queue<GameObject>();
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
            
            if (explodePrefab == null)
            {
                Debug.LogError("explodePrefab为空");
            }
            else
            {
                print("explodePrefab加载成功");
                Explode explode = explodePrefab.GetComponent<Explode>();
                if (explode == null)
                {
                    Debug.LogError("explodePrefab预制体上没有explodePrefab组件"); 
                }
            }
            
            _poolContainer = new GameObject("ExplosionPoolContainer").transform;
            _poolContainer.SetParent(transform);
            InitPool();
        }

        private void InitPool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject explosion = Instantiate(explodePrefab, _poolContainer);
                explosion.SetActive(false);
                _explodePool.Enqueue(explosion);
            }
        }

        private GameObject CreateNewExplode()
        {
            return Instantiate(explodePrefab, _poolContainer);
        }


        public GameObject GetExplode()
        {
            GameObject explosion;
            if (_explodePool.Count > 0)
            {
                explosion = _explodePool.Dequeue();
            }
            else
            {
                for (int i = 0; i < poolExpandSize - 1; i++)
                {
                    GameObject newExplosion = CreateNewExplode();
                    newExplosion.SetActive(false);
                    _explodePool.Enqueue(newExplosion);
                }
                explosion = CreateNewExplode();
            }
            explosion.SetActive(true);
            return explosion;
        }

        public void ReturnExplode(GameObject explode)
        {
            explode.SetActive(false);
            explode.transform.SetParent(_poolContainer);
            _explodePool.Enqueue(explode);
            
        }
        
    }
}