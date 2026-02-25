using System.Collections.Generic;
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
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnCreate(Dictionary<string, string> dict)
        {
            // 默认不处理参数
        }
    }
}
