using System;
using System.Collections.Generic;

namespace GameSystem.GameScene.GameEndScene
{
    /// <summary>
    ///     单个玩家的游戏结算数据
    /// </summary>
    [Serializable]
    public class PlayerResultData
    {
        /// <summary>角色ID</summary>
        public string CharacterId;

        /// <summary>角色名称</summary>
        public string CharacterName;

        /// <summary>角色类型</summary>
        public string CharacterType;

        /// <summary>是否存活</summary>
        public bool IsAlive;

        /// <summary>击杀数</summary>
        public int KillCount;

        /// <summary>死亡数（0或1）</summary>
        public int DeathCount;

        /// <summary>最终等级</summary>
        public int FinalLevel;

        /// <summary>最终经验值</summary>
        public int FinalExp;
    }

    /// <summary>
    ///     游戏结算总数据，用于传递给结果面板
    /// </summary>
    [Serializable]
    public class GameResultData
    {
        /// <summary>玩家是否胜利（至少有一名玩家存活）</summary>
        public bool IsPlayerVictory;

        /// <summary>所有参与角色的结算数据</summary>
        public List<PlayerResultData> PlayerResults = new();

        /// <summary>游戏总时长（秒）</summary>
        public float GameDuration;

        /// <summary>地图名称</summary>
        public string MapName;

        /// <summary>游戏模式</summary>
        public string GameMode;
    }
}
