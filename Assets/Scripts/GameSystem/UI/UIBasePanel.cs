using GameSystem.GameScene.MainMenu;
using GameSystem.UI;
using UnityEngine;

namespace GameSystem.GameScene
{
    public abstract class UIBasePanel : MonoBehaviour
    {
        public abstract PanelSymbol symbol { get; }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
            MainUIManager.Instance.RegisterPanel(this);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
