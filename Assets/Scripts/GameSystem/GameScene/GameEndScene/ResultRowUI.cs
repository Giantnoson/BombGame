using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.GameEndScene
{
    /// <summary>
    ///     结算面板中每个玩家的结果行，显示名称、状态、击杀数、等级等
    /// </summary>
    public class ResultRowUI : MonoBehaviour
    {
        [Header("角色信息")]
        [Tooltip("角色名称")]
        public TextMeshProUGUI nameText;

        [Tooltip("角色类型")]
        public TextMeshProUGUI typeText;

        [Header("状态")]
        [Tooltip("存活/阵亡 状态文本")]
        public TextMeshProUGUI statusText;

        [Tooltip("存活状态颜色")]
        public Color aliveColor = Color.green;

        [Tooltip("阵亡状态颜色")]
        public Color deadColor = Color.gray;

        [Header("数据统计")]
        [Tooltip("击杀数")]
        public TextMeshProUGUI killCountText;

        [Tooltip("最终等级")]
        public TextMeshProUGUI levelText;

        [Tooltip("最终经验")]
        public TextMeshProUGUI expText;

        [Header("背景")]
        [Tooltip("行背景图片（可用于高亮）")]
        public Image backgroundImage;

        [Tooltip("存活行背景色")]
        public Color aliveBgColor = new(0.2f, 0.4f, 0.2f, 0.5f);

        [Tooltip("阵亡行背景色")]
        public Color deadBgColor = new(0.3f, 0.1f, 0.1f, 0.5f);

        /// <summary>
        ///     设置单行数据
        /// </summary>
        public void SetData(PlayerResultData data)
        {
            // 角色名称
            if (nameText != null)
                nameText.text = data.CharacterName;

            // 角色类型
            if (typeText != null)
                typeText.text = data.CharacterType;

            // 存活状态
            if (statusText != null)
            {
                statusText.text = data.IsAlive ? "存活" : "阵亡";
                statusText.color = data.IsAlive ? aliveColor : deadColor;
            }

            // 击杀数
            if (killCountText != null)
                killCountText.text = data.KillCount.ToString();

            // 等级
            if (levelText != null)
                levelText.text = $"Lv.{data.FinalLevel}";

            // 经验值
            if (expText != null)
                expText.text = $"{data.FinalExp} EXP";

            // 背景色
            if (backgroundImage != null)
                backgroundImage.color = data.IsAlive ? aliveBgColor : deadBgColor;
        }
    }
}
