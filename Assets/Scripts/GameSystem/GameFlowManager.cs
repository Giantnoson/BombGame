using UnityEngine;

namespace GameSystem
{
    
    [System.Serializable]
    public enum GameState
    {
        MainMenu,//主菜单
        InGame,//游戏中
        GameOver//游戏结束
    }
    public class GameFlowManager : MonoBehaviour
    {

        
        public GameState GameState { get; private set; }
        
        private void Start()
        {
            GameState = GameState.MainMenu;
        }
        
        
        
        
        
        
        
        public static GameFlowManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        
        
        
    }
}