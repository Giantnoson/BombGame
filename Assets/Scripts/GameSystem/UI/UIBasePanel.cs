using System;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace GameSystem.GameScene
{
    public abstract class UIBasePanel : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
        
        
        
    }
}
