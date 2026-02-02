using Config;

namespace GameSystem.EventSystem
{
    public class HUDEvent
    {
        public class LoadHUDEvent : GameEvent
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

            public LoadHUDEvent(string id, string playerName, CharacterType characterType,
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
    }
}