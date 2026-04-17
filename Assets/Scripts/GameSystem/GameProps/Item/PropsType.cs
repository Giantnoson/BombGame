namespace GameSystem.GameProps.Item
{
    /// <summary>
    /// 道具类型
    /// 道具是给角色提供Buff加成的道具
    /// 道具大小以尾缀为准，S代表小，M代表中，L代表大
    /// </summary>
    
    public enum PropsType
    {
        SpeedUpper,
        StaminaUpper,
        BombDamageUpper,
        BombRadiusUpper,
        BombFuseTimeLower,
        BombCooldownLower,
        BombRecoveryTimeLower,
        MaxHpUpper,
        HpRegen,
    }
}