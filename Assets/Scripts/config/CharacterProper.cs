using UnityEngine;

namespace config
{
    [CreateAssetMenu()]
    public class CharacterProper : ScriptableObject
    {
        [Header("角色基础设置")]
        [Tooltip("角色类型")]
        public CharacterType playerType; //角色类型
        [Tooltip("最大生命值")]
        public float maxHp = 100; //最大生命值
        [Tooltip("移动速度")]
        public float speed = 5f; //移动速度
        [Tooltip("最大等级")]
        public int maxLevel = 3; //最大等级

        
        
        [Header("体力设置")]
        [Tooltip("体力上限")]
        public float maxStamina = 100f; //体力上限
        [Tooltip("体力消耗速率")]
        public float staminaDrainRate = 10f; //体力消耗速率
        [Tooltip("体力恢复速率")]
        public float staminaRegenRate = 10f; //体力恢复速率
        [Tooltip("速度倍率")]
        public float speedMultiplier = 1.2f; //速度倍率


        
        
        [Header("炸弹设置")]
        [Tooltip("最大炸弹数")]
        public int maxBombCount = 5; //最大炸弹数量
        [Tooltip("炸弹伤害")]
        public float bombDamage = 20f; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public int bombRadius = 5; //爆炸范围
        [Tooltip("爆炸时间")]
        public float bombFuseTime = 3f;//爆炸时间
        [Tooltip("放置炸弹冷却时间")]
        public float bombCooldown = 0.8f; //放置炸弹冷却时间
        [Tooltip("炸弹恢复时间")]
        public float bombRecoveryTime = 2f;

        
        [Header("成长率设置")]
        [Tooltip("速度成长数值")]
        public float speedGrowth = 0.5f; //速度成长数值
        [Tooltip("体力成长数值")]
        public float staminaGrowth = 10f; //体力成长数值
        [Tooltip("炸弹伤害增加数值")]
        public float bombDamageGrowth = 10f; //炸弹伤害增加数值
        [Tooltip("炸弹爆炸范围增加数值")]
        public int bombRadiusGrowth = 1; //炸弹爆炸范围增加数值
        [Tooltip("炸弹爆炸时间减少数值")]
        public float bombFuseTimeGrowth = 0.5f; //炸弹爆炸时间减少数值
        [Tooltip("炸弹冷却时间减少数值")]
        public float bombCooldownGrowth = 0.1f; //炸弹冷却时间减少数值
        [Tooltip("炸弹恢复时间减少数值")]
        public float bombRecoveryTimeGrowth = 0.5f; //炸弹恢复时间减少数值
        [Tooltip("最大炸弹数增加数值")]
        public int maxBombCountGrowth = 1; //最大炸弹数增加数值
        [Tooltip("最大生命值增加数值")]
        public float maxHpGrowth = 10f; //最大生命值增加数值
        
    }

    
    [System.Serializable]
    public enum CharacterType {
        Balance,
        Speed,
        BombTruck,
        Tank,
        Enemy
    }
}