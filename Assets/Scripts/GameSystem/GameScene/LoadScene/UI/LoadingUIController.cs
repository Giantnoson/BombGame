using GameSystem.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace GameSystem.GameScene.MainMenu.GameScene.LoadScene
{
    public class LoadingUIController : MonoBehaviour
    {
        public GameObject loadingPanel; // 加载界面
        public Image loadingProgressBar; // 进度条
        public TextMeshProUGUI loadingText; // 加载提示文本
        public TextMeshProUGUI loadingBarText;
    
        private void OnEnable()
        {
            // 监听场景加载开始事件
            GameFlowManager.OnSceneLoadStarted += OnSceneLoadStarted;
        
            // 监听场景加载进度事件
            GameFlowManager.OnSceneLoadProgress += OnSceneLoadProgress;
        
            // 监听场景加载完成事件
            GameFlowManager.OnSceneLoadCompleted += OnSceneLoadCompleted;
        }
    
        private void OnDisable()
        {
            // 取消监听场景加载事件
            GameFlowManager.OnSceneLoadStarted -= OnSceneLoadStarted;
            GameFlowManager.OnSceneLoadProgress -= OnSceneLoadProgress;
            GameFlowManager.OnSceneLoadCompleted -= OnSceneLoadCompleted;
        }
    
        private void OnSceneLoadStarted(string sceneName)
        {
            // 显示加载界面
            loadingPanel.gameObject.SetActive(true);
        
            // 重置进度条
            loadingProgressBar.fillAmount = 0f;
        
            // 更新加载文本
            loadingText.text = $"正在加载 {sceneName}...";
            loadingBarText.text = "0%";
        }
    
        private void OnSceneLoadProgress(string sceneName, float progress)
        {
            // 更新进度条
            loadingProgressBar.fillAmount = progress;
            
            loadingText.text = $"正在加载 {sceneName}... {Mathf.RoundToInt(progress * 100)}%";
            loadingBarText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
        
        private void OnSceneLoadCompleted(string sceneName)
        {
            // 隐藏加载界面
            loadingPanel.gameObject.SetActive(false);
        }
    }

}