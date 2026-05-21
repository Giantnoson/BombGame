using System;
using Config;
using GameSystem.GameScene.MainMenu;
using GameSystem.Manager;
using GameSystem.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.PushedScene
{
    public class PauseScenePanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.PauseScenePanel;
        
        
        [Header("基础设置")]
        public string panelName = "PauseScenePanel";
        [Tooltip("退出游戏")]
        public Button exitGameBtn;
        [Tooltip("重新开始")]
        public Button restartBtn;
        [Tooltip("设置")]
        public Button settingBtn;
        [Tooltip("取消")]
        public Button cancelBtn;


        public override void Show()
        {
            base.Show();
            exitGameBtn.onClick.AddListener(OnExitGameBtnClick);
            restartBtn.onClick.AddListener(OnRestartBtnClick);
            settingBtn.onClick.AddListener(OnSettingBtnClick);
            cancelBtn.onClick.AddListener(OnCancelBtnClick);
        }

        public override void Hide()
        {
            base.Hide();
            exitGameBtn.onClick.RemoveListener(OnExitGameBtnClick);
            restartBtn.onClick.RemoveListener(OnRestartBtnClick);
            settingBtn.onClick.RemoveListener(OnSettingBtnClick);
            cancelBtn.onClick.RemoveListener(OnCancelBtnClick);
        }

        private void OnCancelBtnClick()
        {
            GameFlowManager.Instance.ResumeGame();
        }

        private void OnExitGameBtnClick()
        {
            GameFlowManager.Instance.ReturnToMainMenu();
        }

        private void OnRestartBtnClick()
        {
            GameFlowManager.Instance.RestartGame();
        }

        private void OnSettingBtnClick()
        {
            
        }
        
        
        
        
        
        
        
        
        
        
        
        
    }
}
