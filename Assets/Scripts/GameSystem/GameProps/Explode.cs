using GameSystem.GameScene.MainMenu.Pool;

namespace GameSystem.GameScene.MainMenu.GameProps
{
    public class Explode : BaseObject
    {
        public float destroyTime = 0.5f;

        private void Awake()
        {
            id = gameObject.GetInstanceID().ToString();
        }

        private void OnEnable()
        {
            Invoke("ReturnToPool", destroyTime);
        }

        private void ReturnToPool()
        {
            ExplodePool.Instance.ReturnExplode(gameObject);
        }
    }
}