using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu]
    public class PropsProbabilityConfig : ScriptableObject
    {
        [Tooltip("道具概率配置，以权重为核心的配置")] [TextArea(3, 5)]
        public string description;

        [Tooltip("道具生成的整体概率 (0-100)")] [Range(0, 100)]
        public int propsGenerationProbability = 50;

        [Tooltip("道具生成的权重列表，与道具配置列表一一对应")] private List<long> _weightPre = new();

        [Tooltip("具体道具的配置列表")] [SerializeField]
        private List<PropsConfig> propsConfigs = new List<PropsConfig>();


        /// <summary>
        /// 获取道具配置的数量
        /// </summary>
        public int PropsCount => propsConfigs.Count;
        [SerializeField] public long totalWeight = 0;


        public bool isInit = false;

        public void Init()
        {
            // 如果propsConfigs列表为空，尝试从Resources加载所有PropsConfig
            if (propsConfigs.Count == 0)
            {
                // 从Resources/Props目录加载所有PropsConfig资源
                var allPropsConfigs = Resources.LoadAll<PropsConfig>("Props");
        
                if (allPropsConfigs == null || allPropsConfigs.Length == 0)
                {
                    Debug.LogError("未找到任何PropsConfig资源，请确保资源位于Resources/Props目录下");
                    return;
                }
        
                // 将加载的配置添加到列表中
                propsConfigs = new List<PropsConfig>(allPropsConfigs);
                Debug.Log($"成功加载 {propsConfigs.Count} 个PropsConfig资源");
            }
    
            // 初始化权重列表
            _weightPre.Clear();
            totalWeight = 0;
            foreach (var propsConfig in propsConfigs)
            {
                if (propsConfig.weight <= 0)
                {
                    Debug.LogError($"道具配置的权重 {propsConfig.weight} 非法");
                } 
                totalWeight += propsConfig.weight;
                _weightPre.Add(totalWeight);
            }

            if (totalWeight == 0)
            {
                Debug.LogError("道具配置的权重总和为0");
            }
            isInit = true;
        }

        public bool GetPropsConfig(out PropsConfig propsConfig)
        {
            if (!isInit)
            {
                Init();
            }
            
            var propsGenerationProbabilityRandom = Random.Range(0, 100);
            if (propsGenerationProbabilityRandom > propsGenerationProbability)
            {
                propsConfig = null;
                return false;
            }
            var randomWeight = Random.Range(0, totalWeight);
            var index = _weightPre.FindIndex(x => x > randomWeight);
            return GetPropsConfig(index, out propsConfig);
        }
        
        /// <summary>
        /// 获取指定索引的道具配置
        /// </summary>
        public bool GetPropsConfig(int index, out PropsConfig propsConfig)
        {
            if (index < 0 || index >= propsConfigs.Count)
            {
                Debug.LogError($"索引 {index} 超出范围");
                propsConfig = null;
                return false;
            }

            propsConfig = propsConfigs[index];
            return true;
        }
        
    }
}