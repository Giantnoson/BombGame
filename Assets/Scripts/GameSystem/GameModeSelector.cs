using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    public class GameModeSelector : MonoBehaviour
    {
        public static GameMode CurrentMode { get; private set; }

        public void SelectPVPMode()
        {
            CurrentMode = GameMode.PVP;
            LoadGameScene();
        }

        public void SelectPVEMode()
        {
            CurrentMode = GameMode.PVE;
            LoadGameScene();
        }

        private void LoadGameScene()
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}