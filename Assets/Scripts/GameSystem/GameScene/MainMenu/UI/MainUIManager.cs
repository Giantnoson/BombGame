using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
{ 
    public class MainUIManager : BaseUIManager 
    {
        private static MainUIManager _instance;
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
    }
}
