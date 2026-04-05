using GameSystem.GameProps.Item;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class PropsConfig : ScriptableObject
    {
        [Tooltip("有效时长,定时触发为0,一次性为-1")]
        public float validTime;
        [Tooltip("道具类型")]
        public PropsType propsType;
        [Tooltip("道具大小(表示缩放大小)")]
        public float propsSize;
        [Tooltip("道具Obj")]
        public GameObject propsObj;
        [Tooltip("道具描述")]
        public string propsDesc;
        [Tooltip("道具名称")]
        public string propsName;
        [Tooltip("道具图标")]
        public Sprite propsIcon;
        [Tooltip("道具是否可拾取")]
        public bool canPickUp = true;
        
        [Header("角色基础设置")]
        [Tooltip("最大生命值")]
        public float maxHp = 0; //最大生命值
        [Tooltip("移动速度")]
        public float speed = 0; //移动速度
        [Tooltip("最大等级")]
        public int maxLevel = 0; //最大等级
        
        [Header("体力设置")]
        [Tooltip("体力上限")]
        public float maxStamina = 0; //体力上限
        [Tooltip("体力消耗速率")]
        public float staminaDrainRate = 0; //体力消耗速率
        [Tooltip("体力恢复速率")]
        public float staminaRegenRate = 0; //体力恢复速率
        [Tooltip("速度倍率")]
        public float speedMultiplier = 0; //速度倍率
        
        [Header("炸弹设置")]
        [Tooltip("最大炸弹数")]
        public int maxBombCount = 0; //最大炸弹数量
        [Tooltip("炸弹伤害")]
        public float bombDamage = 0; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public int bombRadius = 0; //爆炸范围
        [Tooltip("爆炸时间")]
        public float bombFuseTime = 0;//爆炸时间
        [Tooltip("放置炸弹冷却时间")]
        public float bombCooldown = 0; //放置炸弹冷却时间
        [Tooltip("炸弹恢复时间")]
        public float bombRecoveryTime = 0;

        
        [Header("成长率设置")]
        [Tooltip("速度成长数值")]
        public float speedGrowth = 0; //速度成长数值
        [Tooltip("体力成长数值")]
        public float staminaGrowth = 0; //体力成长数值
        [Tooltip("炸弹伤害增加数值")]
        public float bombDamageGrowth = 0; //炸弹伤害增加数值
        [Tooltip("炸弹爆炸范围增加数值")]
        public int bombRadiusGrowth = 0; //炸弹爆炸范围增加数值
        [Tooltip("炸弹爆炸时间减少数值")]
        public float bombFuseTimeGrowth = 0f; //炸弹爆炸时间减少数值
        [Tooltip("炸弹冷却时间减少数值")]
        public float bombCooldownGrowth = 0f; //炸弹冷却时间减少数值
        [Tooltip("炸弹恢复时间减少数值")]
        public float bombRecoveryTimeGrowth = 0; //炸弹恢复时间减少数值
        [Tooltip("最大炸弹数增加数值")]
        public int maxBombCountGrowth = 0; //最大炸弹数增加数值
        [Tooltip("最大生命值增加数值")]
        public float maxHpGrowth = 0; //最大生命值增加数值
        
        public PropsConfig GetProps(PropsType propsType)
        {
            return Resources.Load<PropsConfig>($"Props/{propsType.ToString()}");
        }

        public void Init(PropsConfig config)
        {
            validTime = config.validTime;
            propsType = config.propsType;
            propsSize = config.propsSize;
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}