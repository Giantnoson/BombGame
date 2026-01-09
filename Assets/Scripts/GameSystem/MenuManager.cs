using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem
{
    public class MenuManager : MonoBehaviour
    {
        public Button pvp;
        public Button pve;
        public Button exit;

        public GameObject mainMenuPanel;

        private GameModeSelector _gameModeSelector;
        private void Start()
        {
            _gameModeSelector = FindObjectOfType<GameModeSelector>();

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            if (pvp != null)
            {
                pvp.onClick.AddListener(SelectPVPMode);
            }

            if (pve != null)
            {
                pve.onClick.AddListener(SelectPVEMode);
            }

            if (exit != null)
            {
                exit.onClick.AddListener(ExitGame);
            }
        }


        private void SelectPVPMode()
        {
            mainMenuPanel.SetActive(false);
            _gameModeSelector.SelectPVPMode();
        }

        private void SelectPVEMode()
        {
            mainMenuPanel.SetActive(false);
            _gameModeSelector.SelectPVEMode();
        }

        private void ExitGame()
        {
            Application.Quit();
        }
    }

}
