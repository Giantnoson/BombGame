using System.Collections.Generic;
using Config;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMapSelectPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MapSelectPanel;
        [Header("UI组件")]
        public Button nextBtn;
        public Button prevBtn;
        public Button continueBtn;
        public Button backButton;
        public TextMeshProUGUI mapNameText;
        //public TextMeshProUGUI mapDescriptionText;
        public Image mapImage;
        
        [Header("地图相关")]
        public List<MapSelectInfo> mapSelectInfoList;
        public int mapIndex;
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

        private void SetMapSelectInfo(int index)
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


        private void OnClickNextBtn()
        {
            mapIndex++;
            mapIndex = mapIndex % mapSelectInfoList.Count;
            SetMapSelectInfo(mapIndex);
        }

        private void OnClickPrevBtnBtn()
        {
            mapIndex--;
            mapIndex = (mapIndex + mapSelectInfoList.Count) % mapSelectInfoList.Count;
            SetMapSelectInfo(mapIndex);
        }

        private void OnContinueBtnClick()
        {
            GameModeSelect.Instance.SetMap(mapSelectInfoList[mapIndex]);
            MainUIManager.Instance.ShowPanel(PanelSymbols.SinglePlayerPanel);
        }
        
        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}