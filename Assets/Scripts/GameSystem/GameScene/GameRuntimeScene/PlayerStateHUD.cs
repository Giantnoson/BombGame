using System;
using player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.GameRuntimeScene
{
    public class PlayerStateHUD : MonoBehaviour
    {
        [Tooltip("所有者")]
        public int ownerId;
        
        [Header("UI显示")]
        [Tooltip("玩家名称文本")]
        public TextMeshProUGUI playerNameText;
        [Tooltip("角色类型")]
        public TextMeshProUGUI playerTypeText;
        [Tooltip("生命值文本")]
        public TextMeshProUGUI hpText;
        [Tooltip("生命值条")]
        public Image hpBar;
        [Tooltip("体力文本")]
        public TextMeshProUGUI staminaText;
        [Tooltip("体力条")]
        public Image staminaBar;
        [Tooltip("经验值文本")]
        public TextMeshProUGUI expText;
        [Tooltip("经验值条")]
        public Image expBar;
        [Tooltip("等级文本")]
        public TextMeshProUGUI leaveText;
        [Tooltip("移动速度文本")]
        public TextMeshProUGUI moveSpeedText;
        [Tooltip("炸弹放置冷却文本")]
        public TextMeshProUGUI bombCooldownText;
        [Tooltip("炸弹爆炸时间文本")]
        public TextMeshProUGUI bombFuseTimeText;
        [Tooltip("炸弹数量文本")]
        public TextMeshProUGUI bombCountText;
        [Tooltip("炸弹伤害文本")]
        public TextMeshProUGUI bombDamageText;
        [Tooltip("炸弹爆炸范围文本")]
        public TextMeshProUGUI bombRadiusText;

        
        public PlayerController PlayerController { get; set; }


        private void Start()
        {
            InitUI();
        }

        private void InitUI()
        {
            // 验证UI引用
            ValidateReference(playerNameText, "playerNameText");
            ValidateReference(playerTypeText, "playerTypeText");
            ValidateReference(hpText, "hpText");
            ValidateReference(hpBar, "hpBar");
            ValidateReference(staminaText, "staminaText");
            ValidateReference(staminaBar, "staminaBar");
            ValidateReference(expText, "expText");
            ValidateReference(expBar, "expBar");
            ValidateReference(bombCountText, "bombCountText");
            ValidateReference(bombDamageText, "bombDamageText");
            ValidateReference(bombRadiusText, "bombRadiusText");
            ValidateReference(leaveText, "leaveText");
            ValidateReference(moveSpeedText, "moveSpeedText");
            ValidateReference(bombCooldownText, "bombCooldownText");
            ValidateReference(bombFuseTimeText, "bombFuseTimeText");
        }

        private void ValidateReference(object obj, string name)
        {
            if (obj == null) Debug.LogError($"{name}为空");
        }
        
        public void LoadHUD(string playerName,PlayType playType , PlayerProper playerProper, GlobalProper globalProper ,float hp, float stamina, int exp, int level, float currentSpeed)
        {
            // 初始化玩家名称
            playerNameText.text = playerName;
            playerTypeText.text = playType.ToString();
            // 初始化HP相关
            hpText.text = $"{hp}/{playerProper.maxHp}";
            hpBar.fillAmount = hp / playerProper.maxHp;
    
            // 初始化体力相关
            staminaText.text = $"{stamina}/{playerProper.maxStamina}";
            staminaBar.fillAmount = stamina / playerProper.maxStamina;
    
            // 初始化经验值相关
            expText.text = $"{exp}/{globalProper.maxExpToLevelUp}";
            expBar.fillAmount = (float)exp / globalProper.maxExpToLevelUp;
    
            // 初始化炸弹相关
            bombCountText.text = $"{globalProper.initBombCount.ToString()}/{playerProper.maxBombCount.ToString()}({playerProper.bombRecoveryTime.ToString("F2")})";
            bombDamageText.text = playerProper.bombDamage.ToString();
            bombRadiusText.text = playerProper.bombRadius.ToString();
            bombFuseTimeText.text = playerProper.bombFuseTime.ToString("F2");
    
            // 初始化其他属性
            leaveText.text = level.ToString();
            moveSpeedText.text = currentSpeed.ToString();
            bombCooldownText.text = playerProper.bombCooldown.ToString("F2");
        }

        public void UpdateStamina(float stamina, float maxStamina, float currentSpeed)
        {
            staminaText.text = $"{stamina.ToString("F2")}/{maxStamina}";
            staminaBar.fillAmount = stamina / maxStamina;
            moveSpeedText.text = currentSpeed.ToString("F2");
        }
        
        public void UpdateBomb(float bombCooldown, int bombCount, int maxBombCount, float bombRecoveryTime)
        { 
            //更新UI
            bombCooldownText.text = bombCooldown.ToString("F2");
            bombCountText.text =$"{bombCount.ToString()}/{maxBombCount.ToString()}({bombRecoveryTime.ToString("F2")})";
        }

        public void OnLeaveUpUIUpdate(float hp, float maxHp, float stamina, float maxStamina,
            int exp, int level,int maxExpToLevelUp, float currentSpeed, int bombCount, int maxBombCount, float bombRecoveryTime, float bombDamage, float bombRadius, float bombFuseTime)
        {
            // ========== 生命值更新 ==========
            hpText.text = $"{hp}/{maxHp}";
            hpBar.fillAmount = hp / maxHp;
    
            // ========== 体力值更新 ==========
            staminaText.text = $"{stamina}/{maxStamina}";
            staminaBar.fillAmount = stamina / maxStamina;
    
            // ========== 经验值更新 ==========
            expText.text = $"{exp}/{maxExpToLevelUp}";
            expBar.fillAmount = (float)exp / maxExpToLevelUp;
    
            // ========== 等级更新 ==========
            leaveText.text = level.ToString();
    
            // ========== 速度更新 ==========
            moveSpeedText.text = currentSpeed.ToString();
    
            // ========== 炸弹属性更新 ==========
            bombCountText.text =$"{bombCount.ToString()}/{maxBombCount.ToString()}({bombRecoveryTime.ToString("F2")})";
            bombDamageText.text = bombDamage.ToString();
            bombRadiusText.text = bombRadius.ToString();
            bombFuseTimeText.text = bombFuseTime.ToString("F2"); // 保留两位小数
        }
        public void OnExpAddUIUpdate(int exp, int maxExpToLevelUp)
        {
            expText.text = $"{exp}/{maxExpToLevelUp}";
            expBar.fillAmount = (float)exp / maxExpToLevelUp;
        }
        
        public void OnTakeDamageUIUpdate(float hp,float maxHp)
        {
            hpText.text = $"{hp}/{maxHp}";
            hpBar.fillAmount = hp / maxHp;
        }

    }
}