using Config;
using UnityEngine;

namespace GameSystem.GameProps.Item
{
    public class PropsManager : MonoBehaviour
    {
        private static PropsManager _instance;
        
        public PropsProbabilityConfig config;
        
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
            config = Resources.Load<PropsProbabilityConfig>("Props/PropsProbabilityConfig");
            if (config == null)
            {
                Debug.LogError("PropsProbabilityConfig not found");
            }
            else
            {
                config.Init();
            }
            //DontDestroyOnLoad(gameObject);
        }

        public bool CreateProps(out PropsConfig propsConfig)
        {
            return config.GetPropsConfig(out propsConfig);
        }

        /// <summary>
        /// 根据道具ID查找对应的 PropsConfig（用于在线模式下服务端下发道具时匹配本地配置）
        /// </summary>
        public bool GetPropsConfigById(string propsId, out PropsConfig propsConfig)
        {
            propsConfig = null;
            if (config == null)
            {
                config = Resources.Load<PropsProbabilityConfig>("Props/PropsProbabilityConfig");
                if (config != null) config.Init();
                else return false;
            }
            var allConfigs = Resources.LoadAll<PropsConfig>("Props");
            foreach (var pc in allConfigs)
            {
                if (pc.propsId == propsId)
                {
                    propsConfig = pc;
                    return true;
                }
            }
            return false;
        }
        
    }
}