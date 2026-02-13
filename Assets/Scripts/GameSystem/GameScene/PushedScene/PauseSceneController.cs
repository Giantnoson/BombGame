using System;
using Config;
using GameSystem.GameScene.MainMenu.EventSystem;
using GameSystem.UI;
using UnityEngine;

namespace GameSystem.GameScene.PushedScene
{
    public class PauseSceneController :MonoBehaviour
    {
        public PauseScenePanel pauseScenePanel;


        private void Start()
        {
            PauseSceneManager.Instance.RegisterPanel(pauseScenePanel);
            
            PauseSceneManager.Instance.CloseAll();
            GameEventSystem.AddListener<GameEvents.PauseGameEvent>(OnPauseGameEvent);
            GameEventSystem.AddListener<GameEvents.ResumeGameEvent>(OnResumeGameEvent);
            GameEventSystem.AddListener<GameEvents.GameStateChangedEvent>(OnGameStateChangedEvent);

        }

        private void OnDestroy()
        {
            GameEventSystem.RemoveListener<GameEvents.PauseGameEvent>(OnPauseGameEvent);
            GameEventSystem.RemoveListener<GameEvents.ResumeGameEvent>(OnResumeGameEvent);
            GameEventSystem.RemoveListener<GameEvents.GameStateChangedEvent>(OnGameStateChangedEvent);
        }


        private void OnPauseGameEvent(GameEvents.PauseGameEvent evt)
        {
            PauseSceneManager.Instance.ShowPanel(PanelSymbols.PauseScenePanel);
        }

        private void OnResumeGameEvent(GameEvents.ResumeGameEvent evt)
        {
            PauseSceneManager.Instance.CloseAll();
        }

        private void OnGameStateChangedEvent(GameEvents.GameStateChangedEvent evt)
        {
            if (evt.newState.state == GameState.Paused)
            {
                PauseSceneManager.Instance.ShowPanel(PanelSymbols.PauseScenePanel);
            }
            else if (evt.oldState.state == GameState.Paused)
            {
                PauseSceneManager.Instance.CloseAll();
            }
        }
        
        
        [ContextMenu("自动加载UI参照")]
        public void AutoSetup()
        {
            pauseScenePanel = GetComponentInChildren<PauseScenePanel>(true);
        }
        
    }
}