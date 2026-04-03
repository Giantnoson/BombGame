using GameSystem.GameScene.MainMenu.GameProps;
using GameSystem.GameScene.MainMenu.Map;
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

        public Bomb GetBomb()
        {
            var bomb = GetObjectFromPool();
            if (bomb != null) bomb.gameObject.SetActive(true);
            return bomb;
        }

        public void ReturnBomb(Bomb bomb)
        {
            ReturnObject(bomb);
        }

        protected override void ResetObject(Bomb bomb)
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