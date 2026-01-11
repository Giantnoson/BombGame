using UnityEngine;

namespace GameSystem
{
    public class GameFlowManager : MonoBehaviour
    {

        
        
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