using GameSystem.GameProps;
using UnityEngine;

namespace GameSystem.Pool
{
    public class BombPool : ObjectPool<Bomb>
    {
        public static BombPool Instance { get; private set; }
        
        protected override void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public Bomb GetBomb()
        {
            var bomb = GetObjectFromPool();
            if (bomb != null)
            {
                // 清理所有残留状态（对象池复用时状态泄漏会导致在线炸弹不爆炸）
                bomb.isExplode = false;
                bomb.isOnlineBomb = false;
                bomb.CancelInvoke("Explode");
                bomb.gameObject.SetActive(true);
            }
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
            // 取消残留的爆炸 Invoke，防止回收后仍触发（SetActive(false) 不会取消 Invoke）
            bomb.CancelInvoke("Explode");
        }
    }
}