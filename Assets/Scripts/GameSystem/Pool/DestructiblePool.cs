using GameSystem.GameProps;

namespace GameSystem.Pool
{
    public class DestructiblePool : ObjectPool<Destructible>
    {
        public static DestructiblePool Instance { get; private set; }
        
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
            initialPoolSize = 10;
            poolExpandSize = 50;
            maxPoolSize = 500;
        }

        public Destructible GetDestructible()
        {
            var destructible = GetObjectFromPool();
            if (destructible != null) destructible.gameObject.SetActive(true);
            return destructible;
        }

        public void ReturnDestructible(Destructible destructible)
        {
            ReturnObject(destructible);
        }

        protected override void ResetObject(Destructible destructible)
        {
            
        }
    }
}