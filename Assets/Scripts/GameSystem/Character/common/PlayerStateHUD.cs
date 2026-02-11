using GameSystem.GameScene.MainMenu.EventSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu.Character
{
    public class PlayerStateHUD : MonoBehaviour
    {
        private void Start()
        {
            InitUI();
        }


        public void LoadHUD(string ownerId)
        {
            this.ownerId = ownerId;
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


        public void LoadHUD(HUDEvent.LoadHUDEvent evt)
        {
            if (ownerId != evt.Id) return;
            // 初始化玩家名称
            playerNameText.text = evt.PlayerName;
            playerTypeText.text = evt.CharacterType.ToString();

            // 初始化HP相关
            print($"HP: {evt.HP}/{evt.CharacterProper.maxHp} fill = {evt.HP / evt.CharacterProper.maxHp}");

            hpText.text = $"{evt.HP}/{evt.CharacterProper.maxHp}";
            hpBar.fillAmount = evt.HP / evt.CharacterProper.maxHp;

            // 初始化体力相关
            staminaText.text = $"{evt.Stamina}/{evt.CharacterProper.maxStamina}";
            staminaBar.fillAmount = evt.Stamina / evt.CharacterProper.maxStamina;
            // 初始化经验值相关
            print(
                $"EXP: {evt.EXP}/{evt.GlobalProper.maxExpToLevelUp} fill = {(float)evt.EXP / evt.GlobalProper.maxExpToLevelUp}");
            expText.text = $"{evt.EXP}/{evt.GlobalProper.maxExpToLevelUp}";
            expBar.fillAmount = (float)evt.EXP / evt.GlobalProper.maxExpToLevelUp;

            // 初始化炸弹相关
            bombCountText.text =
                $"{evt.GlobalProper.initBombCount.ToString()}/{evt.CharacterProper.maxBombCount.ToString()}({evt.CharacterProper.bombRecoveryTime.ToString("F2")})";
            bombDamageText.text = evt.CharacterProper.bombDamage.ToString();
            bombRadiusText.text = evt.CharacterProper.bombRadius.ToString();
            bombFuseTimeText.text = evt.CharacterProper.bombFuseTime.ToString("F2");

            // 初始化其他属性
            leaveText.text = evt.Level.ToString();

            print($"当前速度： {evt.CurrentSpeed.ToString()}");
            moveSpeedText.text = evt.CurrentSpeed.ToString("F2");
            bombCooldownText.text = evt.CharacterProper.bombCooldown.ToString("F2");
        }

        public void UpdateStamina(HUDEvent.UpdateStaminaEvent evt)
        {
            if (ownerId != evt.Id) return;
            staminaText.text = $"{evt.Stamina.ToString("F2")}/{evt.MaxStamina}";
            staminaBar.fillAmount = evt.Stamina / evt.MaxStamina;
            moveSpeedText.text = evt.CurrentSpeed.ToString("F2");
        }

        public void UpdateBomb(HUDEvent.UpdateBombEvent evt)
        {
            if (ownerId != evt.Id) return;
            //更新UI
            bombCooldownText.text = evt.BombCooldown.ToString("F2");
            bombCountText.text =
                $"{evt.BombCount.ToString()}/{evt.MaxBombCount.ToString()}({evt.BombRecoveryTime.ToString("F2")})";
        }

        public void OnLeaveUpUIUpdate(HUDEvent.LeaveUpEvent evt)
        {
            if (ownerId != evt.Id) return;

            // ========== 生命值更新 ==========
            hpText.text = $"{evt.HP}/{evt.MaxHp}";
            hpBar.fillAmount = evt.HP / evt.MaxHp;

            // ========== 体力值更新 ==========
            staminaText.text = $"{evt.Stamina}/{evt.MaxStamina}";
            staminaBar.fillAmount = evt.Stamina / evt.MaxStamina;

            // ========== 经验值更新 ==========
            expText.text = $"{evt.EXP}/{evt.MaxExpToLevelUp}";
            expBar.fillAmount = (float)evt.EXP / evt.MaxExpToLevelUp;

            // ========== 等级更新 ==========
            leaveText.text = evt.Level.ToString();

            // ========== 速度更新 ==========
            moveSpeedText.text = evt.CurrentSpeed.ToString();

            // ========== 炸弹属性更新 ==========
            bombCountText.text =
                $"{evt.BombCount.ToString()}/{evt.MaxBombCount.ToString()}({evt.BombRecoveryTime.ToString("F2")})";
            bombDamageText.text = evt.BombDamage.ToString();
            bombRadiusText.text = evt.BombRadius.ToString();
            bombFuseTimeText.text = evt.BombFuseTime.ToString("F2");
        }

        public void OnExpAddUIUpdate(HUDEvent.ExpAddEvent evt)
        {
            if (ownerId != evt.Id) return;
            expText.text = $"{evt.Exp}/{evt.MaxExpToLevelUp}";
            expBar.fillAmount = (float)evt.Exp / evt.MaxExpToLevelUp;
        }

        public void OnTakeDamageUIUpdate(HUDEvent.TakeDamageEvent evt)
        {
            if (ownerId != evt.Id) return;
            hpText.text = $"{evt.HP}/{evt.MaxHp}";
            hpBar.fillAmount = evt.HP / evt.MaxHp;
        }

        #region 基础内容

        [Header("配置")] [Tooltip("所有者")] public string ownerId;

        [Header("UI显示")] [Tooltip("玩家名称文本")] public TextMeshProUGUI playerNameText;

        [Tooltip("角色类型")] public TextMeshProUGUI playerTypeText;

        [Tooltip("生命值文本")] public TextMeshProUGUI hpText;

        [Tooltip("生命值条")] public Image hpBar;

        [Tooltip("体力文本")] public TextMeshProUGUI staminaText;

        [Tooltip("体力条")] public Image staminaBar;

        [Tooltip("经验值文本")] public TextMeshProUGUI expText;

        [Tooltip("经验值条")] public Image expBar;

        [Tooltip("等级文本")] public TextMeshProUGUI leaveText;

        [Tooltip("移动速度文本")] public TextMeshProUGUI moveSpeedText;

        [Tooltip("炸弹放置冷却文本")] public TextMeshProUGUI bombCooldownText;

        [Tooltip("炸弹爆炸时间文本")] public TextMeshProUGUI bombFuseTimeText;

        [Tooltip("炸弹数量文本")] public TextMeshProUGUI bombCountText;

        [Tooltip("炸弹伤害文本")] public TextMeshProUGUI bombDamageText;

        [Tooltip("炸弹爆炸范围文本")] public TextMeshProUGUI bombRadiusText;

        #endregion


        #region 事件监听

        private void OnEnable()
        {
            GameEventSystem.AddListener<HUDEvent.LoadHUDEvent>(LoadHUD);
            GameEventSystem.AddListener<HUDEvent.UpdateStaminaEvent>(UpdateStamina);
            GameEventSystem.AddListener<HUDEvent.UpdateBombEvent>(UpdateBomb);
            GameEventSystem.AddListener<HUDEvent.LeaveUpEvent>(OnLeaveUpUIUpdate);
            GameEventSystem.AddListener<HUDEvent.ExpAddEvent>(OnExpAddUIUpdate);
            GameEventSystem.AddListener<HUDEvent.TakeDamageEvent>(OnTakeDamageUIUpdate);
        }

        private void OnDisable()
        {
            GameEventSystem.RemoveListener<HUDEvent.LoadHUDEvent>(LoadHUD);
            GameEventSystem.RemoveListener<HUDEvent.UpdateStaminaEvent>(UpdateStamina);
            GameEventSystem.RemoveListener<HUDEvent.UpdateBombEvent>(UpdateBomb);
            GameEventSystem.RemoveListener<HUDEvent.LeaveUpEvent>(OnLeaveUpUIUpdate);
            GameEventSystem.RemoveListener<HUDEvent.ExpAddEvent>(OnExpAddUIUpdate);
            GameEventSystem.RemoveListener<HUDEvent.TakeDamageEvent>(OnTakeDamageUIUpdate);
        }

        #endregion
    }
}