using System;
using System.Collections.Generic;
using Config;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
{
    public abstract class UIBasePanel : MonoBehaviour
    {

        public static int SelectedGameScene;
        
        private void OnEnable()
        {

        }

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
