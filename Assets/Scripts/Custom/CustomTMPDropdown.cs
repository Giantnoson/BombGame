using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Custom
{
    public class CustomTMPDropdown : TMP_Dropdown
    {
        
        // 扩展的选项数据，包含是否禁用的标志
        [System.Serializable]
        public new class OptionData
        {
            public string text;
            public Sprite image;
            public bool isDisabled = false; // 新增字段

            public OptionData(string text, Sprite image, bool disabled = false)
            {
                this.text = text;
                this.image = image;
                this.isDisabled = disabled;
            }
        }

        public List<OptionData> customOptions = new List<OptionData>();

        // 重写 Show 方法，实例化选项时设置交互状态
        public new void Show()
        {
            base.Show();
            // 获取生成的下拉列表中的选项 Toggle
            Transform content = transform.Find("Dropdown List/Viewport/Content");
            Toggle[] toggles = content.GetComponentsInChildren<Toggle>(true);

            // 遍历并根据 customOptions 中的 isDisabled 设置 Toggle 的 interactable
            for (int i = 0; i < toggles.Length && i < customOptions.Count; i++)
            {
                toggles[i].interactable = !customOptions[i].isDisabled;
                // 可选：改变视觉效果，如置灰文字，需要额外处理
            }
        }        
        
        
        
        
        
        
        
    }
}