using GameSystem.UI;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIController : MonoBehaviour
    {
        private static MainUIController _instance;
        
        Transform panelParent;
        
        private void Start()
        {
            _instance = this;
            Reset();
        }

        public static void AddPanel(GameObject panel)
        {
            if (_instance.panelParent == null)
            {
                _instance.panelParent = _instance.transform.Find("UI").transform.Find("Panels");
            }
            panel.transform.SetParent(_instance.panelParent, false);
        }

        public static void Reset()
        {
            var manager = MainUIManager.Instance;
            manager.CloseAll();
            manager.ShowPanel(PanelSymbols.BgPanel, true);
            manager.ShowPanel(PanelSymbols.MainPanel);
        }
    }
}