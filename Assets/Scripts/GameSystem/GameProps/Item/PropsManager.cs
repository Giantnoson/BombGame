using UnityEngine;

namespace GameSystem.GameProps.Item
{
    public class PropsManager : MonoBehaviour
    {
        private static PropsManager _instance;
        
        public static PropsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PropsManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PropsManager");
                        _instance = go.AddComponent<PropsManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        public void CreateProps()
        {
            // TODO: 创建道具
            
        }
        
    }
}