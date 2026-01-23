using config;
using player;

namespace GameSystem.EventSystem
{
    public class HUDEvent
    {
        public class LoadHUDEvent : GameEvent
        {
            public LoadHUDEvent(string ownerId, string playerName,  CharacterType characterType, CharacterProper characterProper, GlobalProper globalProper, float hp, float stamina, int exp, int level, float currentSpeed)
            {
                OwnerId = ownerId;
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

            public string PlayerName;
            public CharacterType CharacterType;
            public CharacterProper CharacterProper;
            public GlobalProper GlobalProper;
            public float HP;
            public float Stamina;
            public int EXP;
            public int Level;
            public float CurrentSpeed;
        }
        public class UpdateStaminaEvent : GameEvent
        {
            public UpdateStaminaEvent(string ownerId, float stamina, float maxStamina, float currentSpeed)
            {
                OwnerId = ownerId;
                Stamina = stamina;
                MaxStamina = maxStamina;
                CurrentSpeed = currentSpeed;
            }

            public float Stamina;
            public float MaxStamina;
            public float CurrentSpeed;
        }
        
        public class UpdateBombEvent : GameEvent
        {
            public UpdateBombEvent(string ownerId, float bombCooldown, int bombCount, int maxBombCount, float bombRecoveryTime)
            {
                OwnerId = ownerId;
                BombCooldown = bombCooldown;
                BombCount = bombCount;
                MaxBombCount = maxBombCount;
                BombRecoveryTime = bombRecoveryTime;
            }

            public float BombCooldown;
            public int BombCount;
            public int MaxBombCount;
            public float BombRecoveryTime;
        }
        public class LeaveUpEvent : GameEvent
        {
            public LeaveUpEvent(string ownerId, float hp, float maxHp, float stamina, float maxStamina, int exp, int level, int maxExpToLevelUp, float currentSpeed, int bombCount, int maxBombCount, float bombRecoveryTime, float bombDamage, float bombRadius, float bombFuseTime)
            {
                OwnerId = ownerId;
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

            public float HP;
            public float MaxHp;
            public float Stamina;
            public float MaxStamina;
            public int EXP;
            public int Level;
            public int MaxExpToLevelUp;
            public float CurrentSpeed;
            public int BombCount;
            public int MaxBombCount;
            public float BombRecoveryTime;
            public float BombDamage;
            public float BombRadius;
            public float BombFuseTime;
        }

        public class ExpAddEvent : GameEvent
        {
            public ExpAddEvent(string ownerId, int exp, int maxExpToLevelUp)
            {
                OwnerId = ownerId;
                Exp = exp;
                MaxExpToLevelUp = maxExpToLevelUp;
            }

            public int Exp;
            public int MaxExpToLevelUp;
        }

        public class TakeDamageEvent : GameEvent
        {
            public TakeDamageEvent(string ownerId, float hp, float maxHp)
            {
                OwnerId = ownerId;
                HP = hp;
                MaxHp = maxHp;
            }

            public float HP;
            public float MaxHp;
        }



    }
}