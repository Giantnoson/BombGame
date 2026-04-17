using GameSystem.GameProps.Item;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu()]
    public class PropsConfig : ScriptableObject
    {
        [Header("道具配置")]
        [Tooltip("道具ID")]
        public string propsId;
        [Tooltip("生成权重")]
        [Range(0,2000)]
        public int weight = 60;
        [Tooltip("有效时长,定时触发为0,一次性为-1")]
        [Range(-1, 300)]
        public float validTime = 60;
        [Tooltip("道具类型")]
        
        public PropsType propsType;
        [Tooltip("道具品类,大中小自动控制道具大小")]
        public PropsCategory propsCategory;
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
        //Multiply  乘法
        //Addition  加法
        //Subtract  减法
        //Divide    除法
        [Header("角色基础设置")]
        [Tooltip("最大生命值")]
        public float maxHpAddition = 0; //最大生命值
        [Tooltip("生命恢复")]
        public float hpRegenAddition = 0; //生命恢复
        [Tooltip("移动速度")]
        public float speedMultiply = 0; //移动速度
        [Tooltip("最大等级")]
        public int maxLevelAddition = 0; //最大等级
        
        [Header("体力设置")]
        [Tooltip("体力上限")]
        public float maxStaminaAddition = 0; //体力上限
        [Tooltip("体力消耗速率")]
        public float staminaDrainRateAddition = 0; //体力消耗速率
        [Tooltip("体力恢复速率")]
        public float staminaRegenRateAddition = 0; //体力恢复速率
        [Tooltip("速度倍率")]
        public float speedMultiplierMultiply = 0; //速度倍率
        
        [Header("炸弹设置")]
        [Tooltip("最大炸弹数")]
        public int maxBombCountAddition = 0; //最大炸弹数量
        [Tooltip("炸弹伤害")]
        public float bombDamageAddition = 0; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public int bombRadiusAddition = 0; //爆炸范围
        [Tooltip("爆炸时间")]
        public float bombFuseTimeSubtract = 0;//爆炸时间
        [Tooltip("放置炸弹冷却时间")]
        public float bombCooldownDivide = 0; //放置炸弹冷却时间
        [Tooltip("炸弹恢复时间")]
        public float bombRecoveryTimeDivide = 0;

        public void Init(PropsConfig config)
        {
            validTime = config.validTime;
            propsType = config.propsType;
        }
    }
}