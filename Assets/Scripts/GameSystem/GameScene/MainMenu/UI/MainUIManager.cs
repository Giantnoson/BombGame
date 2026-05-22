using System.Collections.Generic;
using GameSystem.UI;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.UI
{ 
    public class MainUIManager : BaseUIManager 
    {
        private static MainUIManager _instance;

        public Camera mainUICamera;

        
        public static MainUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MainUIManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MainUIManager");
                        _instance = go.AddComponent<MainUIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public override T GetPanel<T>(PanelSymbol symbol)
        {
            return base.GetPanel<T>(symbol);
        }

        public override void ShowPanel(PanelSymbol symbol, bool dontHide = false, Dictionary<string, string> parameters = null)
        {
            base.ShowPanel(symbol, dontHide, parameters);
            mainUICamera.gameObject.SetActive(true);
        }

        public override void Back()
        {
            base.Back();
        }

        public override void BackForDontHide()
        {
            base.BackForDontHide();
        }

        public override void CloseAll()
        {
            base.CloseAll();
            mainUICamera.gameObject.SetActive(false);
        }

        public override void HidePanel()
        {
            base.HidePanel();
            mainUICamera.gameObject.SetActive(false);
        }

        public override void UnHidePanel()
        {
            base.UnHidePanel();
            mainUICamera.gameObject.SetActive(true);
        }
    }
}
