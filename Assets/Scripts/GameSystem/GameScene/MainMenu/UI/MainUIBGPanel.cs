using System;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIBGPanel : UIBasePanel
    {
        public string panelName = "BG";
        private void Start()
        {
            MainUIManager.Instance.RegisterPanel(panelName, this);
        }
    }
}