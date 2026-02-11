using GameSystem.GameScene.MainMenu.EventSystem;
using UnityEngine;

namespace GameSystem.GameScene.PushedScene
{
    public class PauseSceneManager : BaseUIManager
    {

        private static PauseSceneManager _instance;
        public static PauseSceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PauseSceneManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PauseSceneManager");
                        _instance = go.AddComponent<PauseSceneManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
    }
}