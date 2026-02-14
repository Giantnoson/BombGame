using System.Collections.Generic;
using Config;
using Core.Net;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu.UI
{
    public class MainUIMultiPlayerMapSelectPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerMapSelectPanel;
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
        public string mapDescription;
        public Sprite mapSprite;
        public string mapName;

        private bool isMatching;
        
        
        private void Start()
        {
            mapSelectInfoList = MapSelectInfoList.LoadMapSelectInfoLists(MapSelectInfoList.BaseConfig);
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
            
            isMatching = false;
            continueBtn.GetComponentInChildren<TextMeshProUGUI>().text = "开始匹配";
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
            // GameModeSelect.Instance.SetMap(mapSelectInfoList[mapIndex]);
            // MainUIManager.Instance.ShowPanel(PanelSymbols.SinglePlayerPanel);
            if (isMatching)
            {
                isMatching = false;
                continueBtn.GetComponentInChildren<TextMeshProUGUI>().text = "开始匹配";
                TcpGameClient.SendMessage(new Message(CmdType.BaseGameCancelMatch));
                return;
            }
            TcpGameClient.SendMessage(new Message(CmdType.BaseGameStartMatch, new Dictionary<string, object>
            {
                {"id", mapSelectInfoList[mapIndex].mapId},
                {"career", "Balance"}
            }));
            continueBtn.GetComponentInChildren<TextMeshProUGUI>().text = "取消匹配";
            isMatching = true;
        }
        
        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }
    }
}