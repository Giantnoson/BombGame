using System.Collections.Generic;
using Config;
using TMPro;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
{
    public class LoadProper : MonoBehaviour
    {
        public CharacterProper CharacterProper;
        
        [Header("角色基础设置")]
        [Tooltip("最大生命值")]
        public TextMeshProUGUI maxHpText; //最大生命值 
        [Tooltip("移动速度")]
        public TextMeshProUGUI speedText; //移动速度 
        [Tooltip("最大等级")]
        public TextMeshProUGUI maxLevelText; //最大等级
 
        [Header("体力设置")]
        [Tooltip("体力上限")]
        public TextMeshProUGUI maxStaminaText; //体力上限
        [Tooltip("体力消耗速率")]
        public TextMeshProUGUI staminaDrainRateText; //体力消耗速率 
        [Tooltip("体力恢复速率")]
        public TextMeshProUGUI staminaRegenRateText; //体力恢复速率
        [Tooltip("速度倍率")]
        public TextMeshProUGUI speedMultiplierText; //速度倍率
 
        [Header("炸弹设置")]
        [Tooltip("最大炸弹数")]
        public TextMeshProUGUI maxBombCountText; //最大炸弹数量 
        [Tooltip("炸弹伤害")]
        public TextMeshProUGUI bombDamageText; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public TextMeshProUGUI bombRadiusText; //爆炸范围
        [Tooltip("爆炸时间")]
        public TextMeshProUGUI bombFuseTimeText; //爆炸时间
        [Tooltip("放置炸弹冷却时间")]
        public TextMeshProUGUI bombCooldownText; //放置炸弹冷却时间 
        [Tooltip("炸弹恢复时间")]
        public TextMeshProUGUI bombRecoveryTimeText;
 
        [Header("成长率设置")]
        [Tooltip("速度成长数值")]
        public TextMeshProUGUI speedGrowthText; //速度成长数值
        [Tooltip("体力成长数值")]
        public TextMeshProUGUI staminaGrowthText; //体力成长数值 
        [Tooltip("炸弹伤害增加数值")]
        public TextMeshProUGUI bombDamageGrowthText; //炸弹伤害增加数值 
        [Tooltip("炸弹爆炸范围增加数值")]
        public TextMeshProUGUI bombRadiusGrowthText; //炸弹爆炸范围增加数值 
        [Tooltip("炸弹爆炸时间减少数值")]
        public TextMeshProUGUI bombFuseTimeGrowthText; //炸弹爆炸时间减少数值 
        [Tooltip("炸弹冷却时间减少数值")]
        public TextMeshProUGUI bombCooldownGrowthText; //炸弹冷却时间减少数值 
        [Tooltip("炸弹恢复时间减少数值")]
        public TextMeshProUGUI bombRecoveryTimeGrowthText; //炸弹恢复时间减少数值
        [Tooltip("最大炸弹数增加数值")]
        public TextMeshProUGUI maxBombCountGrowthText; //最大炸弹数增加数值
        [Tooltip("最大生命值增加数值")]
        public TextMeshProUGUI maxHpGrowthText; //最大生命值增加数值 

        
        public GameObject prefab;
        [ContextMenu("自动创建属性文本框")]
        public void createTextMenu()
        {
            List<string> list = new List<string>()
            {
                "maxHp",
                "speed",
                "maxLevel",
                "maxStamina",
                "staminaDrainRate",
                "staminaRegenRate",
                "speedMultiplier",
                "maxBombCount",
                "bombDamage",
                "bombRadius",
                "bombFuseTime",
                "bombCooldown",
                "bombRecoveryTime",
                "speedGrowth",
                "staminaGrowth",
                "bombDamageGrowth",
                "bombRadiusGrowth",
                "bombFuseTimeGrowth",
                "bombCooldownGrowth",
                "bombRecoveryTimeGrowth",
                "maxBombCountGrowth",
                "maxHpGrowth"
            };

            foreach (string s in list)
            {
                var go = GameObject.Instantiate(prefab);
                prefab.gameObject.name = s;
                go.transform.SetParent(transform);
            }
        }

        [ContextMenu("加载属性")]
        public void Load()
        {
            var characterProper = CharacterProper;
            
            // 角色基础设置
            maxHpText.text = $"最大血量: {characterProper.maxHp.ToString()}";
            speedText.text = $"移动速度: {characterProper.speed.ToString("F2")}";
            maxLevelText.text = $"最大等级: {characterProper.maxLevel.ToString()}";
            
            // 体力设置
            maxStaminaText.text = $"体力上限: {characterProper.maxStamina.ToString("F2")}";
            staminaDrainRateText.text = $"体力消耗速率: {characterProper.staminaDrainRate.ToString("F2")}";
            staminaRegenRateText.text = $"体力恢复速率: {characterProper.staminaRegenRate.ToString("F2")}";
            speedMultiplierText.text = $"速度倍率: {characterProper.speedMultiplier.ToString("F2")}";
            
            // 炸弹设置
            maxBombCountText.text = $"最大炸弹数: {characterProper.maxBombCount.ToString()}";
            bombDamageText.text = $"炸弹伤害: {characterProper.bombDamage.ToString("F2")}";
            bombRadiusText.text = $"爆炸范围: {characterProper.bombRadius.ToString()}";
            bombFuseTimeText.text = $"爆炸时间: {characterProper.bombFuseTime.ToString("F2")}秒";
            bombCooldownText.text = $"放置冷却: {characterProper.bombCooldown.ToString("F2")}秒";
            bombRecoveryTimeText.text = $"炸弹恢复: {characterProper.bombRecoveryTime.ToString("F2")}秒";
            
            // 成长率设置
            speedGrowthText.text = $"速度成长: {characterProper.speedGrowth.ToString("F2")}";
            staminaGrowthText.text = $"体力成长: {characterProper.staminaGrowth.ToString("F2")}";
            bombDamageGrowthText.text = $"炸弹伤害成长: {characterProper.bombDamageGrowth.ToString("F2")}";
            bombRadiusGrowthText.text = $"爆炸范围成长: {characterProper.bombRadiusGrowth.ToString()}";
            bombFuseTimeGrowthText.text = $"爆炸时间减少: {characterProper.bombFuseTimeGrowth.ToString("F2")}秒";
            bombCooldownGrowthText.text = $"冷却时间减少: {characterProper.bombCooldownGrowth.ToString("F2")}秒";
            bombRecoveryTimeGrowthText.text = $"恢复时间减少: {characterProper.bombRecoveryTimeGrowth.ToString("F2")}秒";
            maxBombCountGrowthText.text = $"炸弹数成长: {characterProper.maxBombCountGrowth.ToString()}";
            maxHpGrowthText.text = $"最大血量成长: {characterProper.maxHpGrowth.ToString("F2")}";
        }
        
        public void Load(CharacterProper characterProper)
        {
            CharacterProper = characterProper;
            
            // 角色基础设置
            maxHpText.text = $"最大血量: {characterProper.maxHp.ToString()}";
            speedText.text = $"移动速度: {characterProper.speed.ToString("F2")}";
            maxLevelText.text = $"最大等级: {characterProper.maxLevel.ToString()}";
            
            // 体力设置
            maxStaminaText.text = $"体力上限: {characterProper.maxStamina.ToString("F2")}";
            staminaDrainRateText.text = $"体力消耗速率: {characterProper.staminaDrainRate.ToString("F2")}";
            staminaRegenRateText.text = $"体力恢复速率: {characterProper.staminaRegenRate.ToString("F2")}";
            speedMultiplierText.text = $"速度倍率: {characterProper.speedMultiplier.ToString("F2")}";
            
            // 炸弹设置
            maxBombCountText.text = $"最大炸弹数: {characterProper.maxBombCount.ToString()}";
            bombDamageText.text = $"炸弹伤害: {characterProper.bombDamage.ToString("F2")}";
            bombRadiusText.text = $"爆炸范围: {characterProper.bombRadius.ToString()}";
            bombFuseTimeText.text = $"爆炸时间: {characterProper.bombFuseTime.ToString("F2")}秒";
            bombCooldownText.text = $"放置冷却: {characterProper.bombCooldown.ToString("F2")}秒";
            bombRecoveryTimeText.text = $"炸弹恢复: {characterProper.bombRecoveryTime.ToString("F2")}秒";
            
            // 成长率设置
            speedGrowthText.text = $"速度成长: {characterProper.speedGrowth.ToString("F2")}";
            staminaGrowthText.text = $"体力成长: {characterProper.staminaGrowth.ToString("F2")}";
            bombDamageGrowthText.text = $"炸弹伤害成长: {characterProper.bombDamageGrowth.ToString("F2")}";
            bombRadiusGrowthText.text = $"爆炸范围成长: {characterProper.bombRadiusGrowth.ToString()}";
            bombFuseTimeGrowthText.text = $"爆炸时间减少: {characterProper.bombFuseTimeGrowth.ToString("F2")}秒";
            bombCooldownGrowthText.text = $"冷却时间减少: {characterProper.bombCooldownGrowth.ToString("F2")}秒";
            bombRecoveryTimeGrowthText.text = $"恢复时间减少: {characterProper.bombRecoveryTimeGrowth.ToString("F2")}秒";
            maxBombCountGrowthText.text = $"炸弹数成长: {characterProper.maxBombCountGrowth.ToString()}";
            maxHpGrowthText.text = $"最大血量成长: {characterProper.maxHpGrowth.ToString("F2")}";
        }

        
        
        [ContextMenu("自动加载UI参照")]
        public void AutoSetup()
        {
            maxHpText = GameObject.Find(nameof(CharacterProper.maxHp)).GetComponent<TextMeshProUGUI>();
            speedText = GameObject.Find(nameof(CharacterProper.speed)).GetComponent<TextMeshProUGUI>();
             maxLevelText = GameObject.Find(nameof(CharacterProper.maxLevel)).GetComponent<TextMeshProUGUI>();
             maxStaminaText = GameObject.Find(nameof(CharacterProper.maxStamina)).GetComponent<TextMeshProUGUI>();
             staminaDrainRateText = GameObject.Find(nameof(CharacterProper.staminaDrainRate)).GetComponent<TextMeshProUGUI>();
             staminaRegenRateText = GameObject.Find(nameof(CharacterProper.staminaRegenRate)).GetComponent<TextMeshProUGUI>();
             speedMultiplierText = GameObject.Find(nameof(CharacterProper.speedMultiplier)).GetComponent<TextMeshProUGUI>();
             maxBombCountText = GameObject.Find(nameof(CharacterProper.maxBombCount)).GetComponent<TextMeshProUGUI>();
             bombDamageText = GameObject.Find(nameof(CharacterProper.bombDamage)).GetComponent<TextMeshProUGUI>();
             bombRadiusText = GameObject.Find(nameof(CharacterProper.bombRadius)).GetComponent<TextMeshProUGUI>();
             bombFuseTimeText = GameObject.Find(nameof(CharacterProper.bombFuseTime)).GetComponent<TextMeshProUGUI>();
             bombCooldownText = GameObject.Find(nameof(CharacterProper.bombCooldown)).GetComponent<TextMeshProUGUI>();
             bombRecoveryTimeText = GameObject.Find(nameof(CharacterProper.bombRecoveryTime)).GetComponent<TextMeshProUGUI>();
             speedGrowthText = GameObject.Find(nameof(CharacterProper.speedGrowth)).GetComponent<TextMeshProUGUI>();
             staminaGrowthText = GameObject.Find(nameof(CharacterProper.staminaGrowth)).GetComponent<TextMeshProUGUI>();
             bombDamageGrowthText = GameObject.Find(nameof(CharacterProper.bombDamageGrowth)).GetComponent<TextMeshProUGUI>();
             bombRadiusGrowthText = GameObject.Find(nameof(CharacterProper.bombRadiusGrowth)).GetComponent<TextMeshProUGUI>();
             bombFuseTimeGrowthText = GameObject.Find(nameof(CharacterProper.bombFuseTimeGrowth)).GetComponent<TextMeshProUGUI>();
             bombCooldownGrowthText = GameObject.Find(nameof(CharacterProper.bombCooldownGrowth)).GetComponent<TextMeshProUGUI>();
             bombRecoveryTimeGrowthText = GameObject.Find(nameof(CharacterProper.bombRecoveryTimeGrowth)).GetComponent<TextMeshProUGUI>();
             maxBombCountGrowthText = GameObject.Find(nameof(CharacterProper.maxBombCountGrowth)).GetComponent<TextMeshProUGUI>();
             maxHpGrowthText = GameObject.Find(nameof(CharacterProper.maxHpGrowth)).GetComponent<TextMeshProUGUI>();
             
        }
        
        
        
        
    }
}