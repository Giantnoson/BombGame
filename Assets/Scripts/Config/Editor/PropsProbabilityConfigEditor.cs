using UnityEditor;
using UnityEngine;

namespace Config.Editor
{
    [CustomEditor(typeof(PropsProbabilityConfig))]
    public class PropsProbabilityConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty descriptionProp;
        private SerializedProperty propsGenerationProbabilityProp;
        private SerializedProperty propsConfigsProp;
        private SerializedProperty isInitProp;
        private SerializedProperty totalWeightProp;
        
        private PropsProbabilityConfig config;

        private void OnEnable()
        {
            // 获取序列化属性
            descriptionProp = serializedObject.FindProperty("description");
            propsGenerationProbabilityProp = serializedObject.FindProperty("propsGenerationProbability");
            propsConfigsProp = serializedObject.FindProperty("propsConfigs");
            isInitProp = serializedObject.FindProperty("isInit");
            totalWeightProp = serializedObject.FindProperty("totalWeight");
            
            // 获取目标对象
            config = (PropsProbabilityConfig)target;
        }

        public override void OnInspectorGUI()
        {
            // 更新序列化对象
            serializedObject.Update();
            
            // 绘制默认检查器
            DrawDefaultInspector();
            
            // 添加间距
            EditorGUILayout.Space();
            
            // 添加初始化按钮
            if (GUILayout.Button("初始化配置", GUILayout.Height(30)))
            {
                InitializeConfig();
            }
            
            // 显示配置状态
            EditorGUILayout.Space();
            if (config.isInit)
            {
                EditorGUILayout.HelpBox($"配置已初始化\n道具数量: {config.PropsCount}\n总权重: {config.totalWeight}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("配置未初始化，请点击上方按钮初始化", MessageType.Warning);
            }
            
            // 应用修改
            serializedObject.ApplyModifiedProperties();
        }
        
        private void InitializeConfig()
        {
            // 调用目标对象的Init方法
            config.Init();
            
            // 标记为已修改，确保Unity保存更改
            EditorUtility.SetDirty(config);
            
            // 显示初始化结果
            if (config.isInit)
            {
                Debug.Log($"成功初始化道具概率配置: {config.name}");
            }
            else
            {
                Debug.LogError($"初始化道具概率配置失败: {config.name}");
            }
        }
    }
}
