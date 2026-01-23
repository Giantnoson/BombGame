using UnityEngine;

namespace GameSystem.Character.Enemy
{
    public abstract class StateBase
    {
        [Header("属性设置")]
        [Tooltip("生命值")]
        public float hp;
        [Tooltip("体力")]
        public float stamina = 100f;
        [Tooltip("炸弹数量")]
        public int bombCount = 0;
        [Tooltip("等级")]
        public int level = 1;
        [Tooltip("经验值")]
        public int exp = 0;
        [Tooltip("基础速度")]
        public float baseSpeed;
        [Tooltip("当前速度")]
        public float currentSpeed;
        
        [Header("炸弹设置")]
        [Tooltip("炸弹伤害")]
        public float bombDamage = 20f; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public int bombRadius = 5; //爆炸范围
        [Tooltip("爆炸时间")]
        public float bombFuseTime = 3f;//爆炸时间
        
        
        [Header("自动恢复属性设置")]
        [Tooltip("放置炸弹冷却时间")]
        public float bombCooldown = 0.8f; //放置炸弹冷却时间
        [Tooltip("炸弹恢复时间")]
        public float bombRecoveryTime = 2f;
        [Tooltip("体力消耗速率")]
        public float staminaDrainRate = 10f; //体力消耗速率
        [Tooltip("体力恢复速率")]
        public float staminaRegenRate = 10f; //体力恢复速率
        [Tooltip("速度倍率")]
        public float speedMultiplier = 1.2f; //速度倍率
        
        
        [Header("最大值设置")]
        [Tooltip("最大生命值")]
        public float maxHp = 100; //最大生命值
        [Tooltip("体力上限")]
        public float maxStamina = 100f; //体力上限
        [Tooltip("最大炸弹数")]
        public int maxBombCount = 5; //最大炸弹数量
        public float maxBombRecoveryTime = 2f;
        public float maxBombCooldown = 0.8f; //放置炸弹冷却时间

        
        [Header("角色状态")]
        public bool isDie = false;
        public bool isStaminaEmpty = false;
    }
}