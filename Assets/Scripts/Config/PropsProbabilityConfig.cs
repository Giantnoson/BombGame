using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        /// <summary>
        ///     将当前道具配置导出为服务端可识别的 JSON 格式
        ///     右键点击 ScriptableObject → "Export Props Config to JSON"
        ///     输出到 persistentDataPath/props_config.json，手动复制到服务端 resources/props/ 目录
        /// </summary>
        [ContextMenu("Export Props Config to JSON")]
        public void ExportPropsConfigToJson()
        {
            if (!isInit) Init();

            if (propsConfigs.Count == 0)
            {
                Debug.LogError("[PropsConfigExport] 没有道具配置可导出");
                return;
            }

            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"propsGenerationProbability\":{propsGenerationProbability},");
            sb.Append("\"props\":{");

            for (var i = 0; i < propsConfigs.Count; i++)
            {
                var pc = propsConfigs[i];
                if (i > 0) sb.Append(",");
                sb.Append($"\"{i}\":{{");
                sb.Append($"\"id\":\"{pc.propsId}\",");
                sb.Append($"\"type\":\"{pc.propsType}\",");
                sb.Append($"\"weight\":{pc.weight},");
                sb.Append($"\"validTime\":{pc.validTime},");
                sb.Append($"\"size\":\"{pc.propsSize}\",");
                // 效果值（与服务端 PropsConfig.loadFromJson 字段名严格一致）
                sb.Append($"\"maxHpAddition\":{pc.maxHpAddition},");
                sb.Append($"\"hpRegenAddition\":{pc.hpRegenAddition},");
                sb.Append($"\"speedMultiply\":{pc.speedMultiply},");
                sb.Append($"\"maxLevelAddition\":{pc.maxLevelAddition},");
                sb.Append($"\"maxStaminaAddition\":{pc.maxStaminaAddition},");
                sb.Append($"\"staminaDrainRateAddition\":{pc.staminaDrainRateAddition},");
                sb.Append($"\"staminaRegenRateAddition\":{pc.staminaRegenRateAddition},");
                sb.Append($"\"speedMultiplierMultiply\":{pc.speedMultiplierMultiply},");
                sb.Append($"\"maxBombCountAddition\":{pc.maxBombCountAddition},");
                sb.Append($"\"bombDamageAddition\":{pc.bombDamageAddition},");
                sb.Append($"\"bombRadiusAddition\":{pc.bombRadiusAddition},");
                sb.Append($"\"bombFuseTimeSubtract\":{pc.bombFuseTimeSubtract},");
                sb.Append($"\"bombCooldownDivide\":{pc.bombCooldownDivide},");
                sb.Append($"\"bombRecoveryTimeDivide\":{pc.bombRecoveryTimeDivide}");
                sb.Append("}");
            }

            sb.Append("}}");

            var json = sb.ToString();
            var filePath = Path.Combine(Application.persistentDataPath, "props_config.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"[PropsConfigExport] 道具配置已导出到: {filePath}");
            Debug.Log($"[PropsConfigExport] 共导出 {propsConfigs.Count} 个道具，总权重={totalWeight}");
            Debug.Log($"[PropsConfigExport] 请手动复制此文件到服务端: src/main/resources/props/props_config.json");
        }
        
    }
}