using Config;

namespace GameSystem.EventSystem.Event
{
    public class HUDEvent
    {
        public class InitHUDEvent : GameEvent
        {
            public CharacterProper CharacterProper;
            public CharacterType CharacterType;
            public float CurrentSpeed;
            public int EXP;
            public GlobalProper GlobalProper;
            public float HP;
            public int Level;

            public string PlayerName;
            public float Stamina;

            public InitHUDEvent(string id, string playerName, CharacterType characterType,
                CharacterProper characterProper, GlobalProper globalProper, float hp, float stamina, int exp, int level,
                float currentSpeed)
            {
                Id = id;
                PlayerName = playerName;
                CharacterType = characterType;
                CharacterProper = characterProper;
                GlobalProper = globalProper;
                HP = hp;
                Stamina = stamina;
                EXP = exp;
                Level = level;
                CurrentSpeed = currentSpeed;
            }
        }

        public class UpdateStaminaEvent : GameEvent
        {
            public float CurrentSpeed;
            public float MaxStamina;

            public float Stamina;

            public UpdateStaminaEvent(string id, float stamina, float maxStamina, float currentSpeed)
            {
                Id = id;
                Stamina = stamina;
                MaxStamina = maxStamina;
                CurrentSpeed = currentSpeed;
            }
        }

        public class UpdateBombEvent : GameEvent
        {
            public float BombCooldown;
            public int BombCount;
            public float BombRecoveryTime;
            public int MaxBombCount;

            public UpdateBombEvent(string id, float bombCooldown, int bombCount, int maxBombCount,
                float bombRecoveryTime)
            {
                Id = id;
                BombCooldown = bombCooldown;
                BombCount = bombCount;
                MaxBombCount = maxBombCount;
                BombRecoveryTime = bombRecoveryTime;
            }
        }

        public class LeaveUpEvent : GameEvent
        {
            public int BombCount;
            public float BombDamage;
            public float BombFuseTime;
            public float BombRadius;
            public float BombRecoveryTime;
            public float CurrentSpeed;
            public int EXP;

            public float HP;
            public int Level;
            public int MaxBombCount;
            public int MaxExpToLevelUp;
            public float MaxHp;
            public float MaxStamina;
            public float Stamina;

            public LeaveUpEvent(string id, float hp, float maxHp, float stamina, float maxStamina, int exp, int level,
                int maxExpToLevelUp, float currentSpeed, int bombCount, int maxBombCount, float bombRecoveryTime,
                float bombDamage, float bombRadius, float bombFuseTime)
            {
                Id = id;
                HP = hp;
                MaxHp = maxHp;
                Stamina = stamina;
                MaxStamina = maxStamina;
                EXP = exp;
                Level = level;
                MaxExpToLevelUp = maxExpToLevelUp;
                CurrentSpeed = currentSpeed;
                BombCount = bombCount;
                MaxBombCount = maxBombCount;
                BombRecoveryTime = bombRecoveryTime;
                BombDamage = bombDamage;
                BombRadius = bombRadius;
                BombFuseTime = bombFuseTime;
            }
        }

        public class ExpAddEvent : GameEvent
        {
            public int Exp;
            public int MaxExpToLevelUp;

            public ExpAddEvent(string id, int exp, int maxExpToLevelUp)
            {
                Id = id;
                Exp = exp;
                MaxExpToLevelUp = maxExpToLevelUp;
            }
        }

        public class TakeDamageEvent : GameEvent
        {
            public float HP;
            public float MaxHp;

            public TakeDamageEvent(string id, float hp, float maxHp)
            {
                Id = id;
                HP = hp;
                MaxHp = maxHp;
            }
        }

        /// <summary>
        ///     计数板实时更新事件（击杀/死亡/等级/经验/存活变化时广播，供计数板UI监听）
        /// </summary>
        public class ScoreBoardUpdateEvent : GameEvent
        {
            /// <summary>发生变化的角色ID</summary>
            public string UpdatedPlayerId;
            /// <summary>该角色击杀数</summary>
            public int KillCount;
            /// <summary>该角色死亡数</summary>
            public int DeathCount;
            /// <summary>该角色当前等级</summary>
            public int Level;
            /// <summary>该角色当前经验值</summary>
            public int Exp;
            /// <summary>该角色是否存活</summary>
            public bool IsAlive;
            /// <summary>当前存活玩家数</summary>
            public int CurrentPlayerCount;
            /// <summary>当前存活NPC数</summary>
            public int CurrentNPCCount;

            public ScoreBoardUpdateEvent(string updatedPlayerId, int killCount, int deathCount,
                int level, int exp, bool isAlive, int currentPlayerCount, int currentNPCCount)
            {
                UpdatedPlayerId = updatedPlayerId;
                KillCount = killCount;
                DeathCount = deathCount;
                Level = level;
                Exp = exp;
                IsAlive = isAlive;
                CurrentPlayerCount = currentPlayerCount;
                CurrentNPCCount = currentNPCCount;
            }
        }

        /// <summary>
        ///     全量刷新 HUD 事件（道具启用/禁用时触发，携带所有可变状态）
        /// </summary>
        public class UpdateHUDEvent : GameEvent
        {
            public float HP;
            public float MaxHp;
            public float Stamina;
            public float MaxStamina;
            public int EXP;
            public int MaxExpToLevelUp;
            public int Level;
            public float CurrentSpeed;
            public int BombCount;
            public int MaxBombCount;
            public float BombRecoveryTime;
            public float BombDamage;
            public int BombRadius;
            public float BombFuseTime;
            public float BombCooldown;
            public float MaxBombCooldown;

            public UpdateHUDEvent(string id, float hp, float maxHp, float stamina, float maxStamina,
                int exp, int maxExpToLevelUp, int level, float currentSpeed,
                int bombCount, int maxBombCount, float bombRecoveryTime,
                float bombDamage, int bombRadius, float bombFuseTime, float bombCooldown, float maxBombCooldown)
            {
                Id = id;
                HP = hp;
                MaxHp = maxHp;
                Stamina = stamina;
                MaxStamina = maxStamina;
                EXP = exp;
                MaxExpToLevelUp = maxExpToLevelUp;
                Level = level;
                CurrentSpeed = currentSpeed;
                BombCount = bombCount;
                MaxBombCount = maxBombCount;
                BombRecoveryTime = bombRecoveryTime;
                BombDamage = bombDamage;
                BombRadius = bombRadius;
                BombFuseTime = bombFuseTime;
                BombCooldown = bombCooldown;
                MaxBombCooldown = maxBombCooldown;
            }
        }
    }
}