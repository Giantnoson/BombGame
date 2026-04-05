namespace GameSystem.Pool
{
    public class MessagePool : ObjectPool<Message.Message>
    {
        public static MessagePool Instance { get; private set; }
        
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

        public  Message.Message GetMessage()
        {
            return base.GetObjectFromPool();
        }
    }
}