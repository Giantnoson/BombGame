using Config;
using GameSystem.GameScene;
using UnityEngine;

namespace GameSystem.GameProps.Item
{
    public class PropsStatus : BaseObject
    {
        [Tooltip("所有者ID")]
        public string ownerId;
        [Tooltip("有效时长,无限为-1")]
        public float validTime;
        [Tooltip("道具类型")]
        public PropsType propsType;
        [Tooltip("道具大小(表示缩放大小)")]
        public float propsSize;
        [Tooltip("道具属性面板")]
        public PropsConfig propsConfig;

        
    }

    /// <summary>
    /// 道具类型
    /// 道具是给角色提供Buff加成的道具
    /// 道具大小以尾缀为准，S代表小，M代表中，L代表大
    /// </summary>
    public enum PropsType
    {
        SpeedUpperL,
        SpeedUpperM,
        SpeedUpperS,
        StaminaUpperL,
        StaminaUpperM,
        StaminaUpperS,
        BombDamageUpperL,
        BombDamageUpperM,
        BombDamageUpperS,
        BombRadiusUpperL,
        BombRadiusUpperM,
        BombRadiusUpperS,
        BombFuseTimeLowerL,
        BombFuseTimeLowerM,
        BombFuseTimeLowerS,
        BombCooldownLowerL,
        BombCooldownLowerM,
        BombCooldownLowerS,
        BombRecoveryTimeLowerL,
        BombRecoveryTimeLowerM,
        BombRecoveryTimeLowerS,
        MaxHpUpperL,
        MaxHpUpperM,
        MaxHpUpperS,
    }
}

