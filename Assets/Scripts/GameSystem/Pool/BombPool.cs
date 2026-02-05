using GameSystem.GameScene.MainMenu.GameProps;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Pool
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
            }
        }

        public GameObject GetBomb()
        {
            var bomb = GetObjectFromPool();
            if (bomb != null) bomb.SetActive(true);
            return bomb;
        }

        public void ReturnBomb(GameObject bomb)
        {
            ReturnObject(bomb);
        }

        protected override void ResetObject(GameObject bomb)
        {
            var collider = bomb.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = true;
            }
        }
    }
}