using System;
using UnityEditor;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class GameRuntimeData : ScriptableObject
    {
        
        
        public void OnEnable()
        {
            #if UNITY_EDITOR
                if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            #endif
            ResetData();
            // TODO 初始化数据
            throw new NotImplementedException();
        }

        
        private void ResetData()
        {
            
        }
    }
}