using GameSystem.Pool;
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

        public GameObject GetBomb()
        {
            GameObject bomb = GetObjectFromPool();
            if (bomb != null)
            {
                bomb.SetActive(true);
            }
            return bomb;
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
