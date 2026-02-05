namespace GameSystem.GameScene.MainMenu.GameProps
{
    public class Destructible : BaseObject
    {
        private void Awake()
        {
            id = gameObject.GetInstanceID().ToString();
        }
    }
}