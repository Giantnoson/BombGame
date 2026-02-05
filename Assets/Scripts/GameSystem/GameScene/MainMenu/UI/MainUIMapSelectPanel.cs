using System;
using System.Collections.Generic;
using Config;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMapSelectPanel : UIBasePanel
    {
        [Header("UI组件")]
        public string panelName = "MapSelectPanel";
        public Button nextBtn;
        public Button prevBtn;
        public Button continueBtn;
        public Button backButton;
        public TextMeshProUGUI mapNameText;
        //public TextMeshProUGUI mapDescriptionText;
        public Image mapImage;
        
        [Header("地图相关")]
        public List<MapSelectInfo> mapSelectInfoList;
        public int mapIndex = 0;
        public Sprite mapSprite;
        public string mapDescription;
        public string mapName;
        
        
        
        private void Start()
        {
            mapSelectInfoList = Resources.Load<MapSelectInfoList>("BaseConfig/MapSelectInfoList").mapSelectInfoList;
            if (mapSelectInfoList == null || mapSelectInfoList.Count == 0)
            {
                Debug.LogError("MapSelectInfoList为Null或者为空");
                return;
            }

            mapIndex = 0;
            SetMapSelectInfo(mapIndex);
            
            nextBtn.onClick.AddListener(OnClickNextBtn);
            prevBtn.onClick.AddListener(OnClickPrevBtnBtn);
            continueBtn.onClick.AddListener(OnContinueBtnClick);
            backButton.onClick.AddListener(OnBackClick);
        }

        public void SetMapSelectInfo(int index)
        {
            if (index < 0)
            {
                Debug.LogError("index小于0");
            }else if (index >= mapSelectInfoList.Count)
            {
                Debug.LogError("index大于等于mapSelectInfoList.Count");
            }
            mapIndex = index;
            mapSprite = mapSelectInfoList[index].mapSprite;
            mapDescription = mapSelectInfoList[index].mapDescription;
            mapName = mapSelectInfoList[index].mapName;
            mapImage.sprite = mapSprite;
            mapNameText.text = mapName;
            //mapDescriptionText.text = mapDescription;
        }


        public void OnClickNextBtn()
        {
            mapIndex++;
            mapIndex = mapIndex % mapSelectInfoList.Count;
            SetMapSelectInfo(mapIndex);
        }

        public void OnClickPrevBtnBtn()
        {
            mapIndex--;
            mapIndex = (mapIndex + mapSelectInfoList.Count) % mapSelectInfoList.Count;
            SetMapSelectInfo(mapIndex);
        }

        public void OnContinueBtnClick()
        {
            GameModeSelect.Instance.SetMap(mapSelectInfoList[mapIndex]);
            MainUIManager.Instance.ShowPanel("SinglePlayerPanel");
        }
        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}