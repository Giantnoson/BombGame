namespace GameSystem.GameScene.MainMenu.GameProps
{
    public class Destructible : BaseObject
    {
        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}