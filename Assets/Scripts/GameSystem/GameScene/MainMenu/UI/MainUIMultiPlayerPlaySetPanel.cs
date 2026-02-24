using System.Collections.Generic;
using System.Linq;
using Config;
using Core.Net;
using GameSystem.GameScene.MessageScene;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIMultiPlayerPlaySetPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerPlaySetPanel;
        
        public static int MaxCharacterCount = 4;
        [Header("基础设置")]
        [Tooltip("面板名称")]
        public Button nextBtn;
        public Button prevBtn;
        public Button startBtn;
        public Button backButton;
        
        [Header("地图相关")]
        public List<MapSelectInfo> mapSelectInfoList;
        public int mapIndex;
        
        [Tooltip("属性面板加载器")]
        public LoadProper loadProper;
        public List<string> playerHeadStr = new List<string>();
        
        [Header("角色键位设置")]
        [Tooltip("角色键位列表")]
        public List<PlayerControlConfig> moveModeList = new List<PlayerControlConfig>();
        public List<string> moveModeName = new List<string>();
        [Tooltip("角色键位下拉框")]
        public TMP_Dropdown playerMoveSelect;
        [Tooltip("角色键位描述")]
        public TextMeshProUGUI playerMoveDescription;
        public List<bool> isMoveModeSelect = new List<bool>{false,false,false,false};
        public PlayerControlConfig CompareTemp;
        
        [Header("角色类型设置")]
        [Tooltip("角色类型名称")]
        public List<string> playerTypeName = new List<string>();
        [Tooltip("角色类型下拉框")]
        public TMP_Dropdown playTypesSelect;
        [Tooltip("角色类型描述")]
        public TextMeshProUGUI playTypesDescription;
        
        [Header("角色基础信息")]
        [Tooltip("角色类型列表")]
        public List<CharacterType> playTypes = new List<CharacterType>();
        [Tooltip("角色名称列表")]
        public List<string> playerNames = new List<string>(MaxCharacterCount);
        [Tooltip("角色ID列表")]
        public List<string> playerIds = new List<string>(MaxCharacterCount);
        [Tooltip("角色键位信息列表")]
        public List<PlayerControlConfig> playerMoveMode = new List<PlayerControlConfig>(MaxCharacterCount);
        [Tooltip("当前索引")]
        public int playerIndex = 0;
        public int playerCount = 0;

        public string mapDescription;
        public Sprite mapSprite;
        public string mapName;
        public TextMeshProUGUI mapNameText;
        public Image mapImage;

        private bool isMatching;
        
        public void RefreshPlayerInfos()
        {
            //角色键位刷新
            if (playerMoveMode[playerIndex].putBomb == KeyCode.None)
            {
                playerMoveDescription.text = playerMoveMode[0].description;
                playerMoveSelect.value = 0;
            }
            else
            {
                playerMoveDescription.text = playerMoveMode[playerIndex].description;
                playerMoveSelect.value = moveModeList.FindIndex(index => index == playerMoveMode[playerIndex]);
            }
            
            //角色类型刷新
            var x = Resources.Load<CharacterProper>("Character/" + playTypes[playerIndex]);
            if (x == null)
            {
                GlobalMessageManager.Instance.SendTopMessage(MessageType.System,MessageLevel.Error, "没有找到该角色属性");
                Debug.LogError("没有找到该角色属性");
            }
            loadProper.Load(x);
            playTypesDescription.text = x.characterDescription;
            playTypesSelect.value = playerTypeName.FindIndex(index => index == playTypes[playerIndex].ToString());
            
        }
        
        public override void Show()
        {
            base.Show();
            mapSelectInfoList = MapSelectInfoList.LoadMapSelectInfoLists(MapSelectInfoList.OnLineConfig);
            if (mapSelectInfoList == null || mapSelectInfoList.Count == 0)
            {
                Debug.LogError("MapSelectInfoList为Null或者为空");
                return;
            }

            mapIndex = 0;
            SetMapSelectInfo(mapIndex);

            playerCount = 1;
            //初始化角色ID
            var x = Resources.Load<PlayerControlList>("PlayerControl/PlayerControlList");
            if (x == null)
            {
                GlobalMessageManager.Instance.SendTopMessage(MessageType.System,MessageLevel.Error, "没有找到该角色键位列表");
                Debug.LogError("没有找到该角色键位列表");
            }
            moveModeList.Clear();
            moveModeList = new List<PlayerControlConfig>(x.playerMoveModeConfigs);
            CompareTemp = moveModeList[OnlineConfig.Instance.defaultControllerId];
            for (int i = 0; i < playerCount; i++)
            {
                playTypes.Add(OnlineConfig.Instance.defaultPlayerType);
                playerNames.Add($"Player{i + 1}");
                playerIds.Add($"P{i + 1}");
                playerMoveMode.Add(CompareTemp);
                playerHeadStr.Add($"玩家{i + 1}" );
            }
            for (int i = playerCount; i < playerCount; i++)
            {
                playTypes.Add(CharacterType.Enemy);
                playerNames.Add($"Enemy{(i - playerCount) + 1}");
                playerIds.Add($"E{i - playerCount + 1}");
                playerMoveMode.Add(CompareTemp);
            }
            
            

            
            //角色类型初始化
            playTypesSelect.ClearOptions();
            playerTypeName = System.Enum.GetValues(typeof(CharacterType))
                .Cast<CharacterType>()
                .Select(v => v.ToString())
                .ToList();
            playTypesSelect.AddOptions(playerTypeName);
            playTypesSelect.onValueChanged.AddListener(OnPlayTypeSelect);
            
            //角色键位初始化

            moveModeName.Clear();
            for (int i = 1; i <= moveModeList.Count; i++)
            {
                moveModeName.Add("移动配置" + i);
            }
            moveModeName.Add("空");
            moveModeList.Add(null);
            
            playerMoveSelect.ClearOptions();
            playerMoveSelect.AddOptions(moveModeName);
            playerMoveSelect.onValueChanged.AddListener(OnPlayerControlSelect);
            playerMoveSelect.value = 0;
            
            nextBtn.onClick.AddListener(OnNextBtnClick);
            prevBtn.onClick.AddListener(OnPrevBtnBtnClick);
            startBtn.onClick.AddListener(OnStartClick);
            backButton.onClick.AddListener(OnBackClick);
            
            RefreshPlayerInfos();
            
            startBtn.GetComponentInChildren<TextMeshProUGUI>().text = "开始匹配";
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

        private void OnPlayerNameInput(string value)
        {
            playerNames[playerIndex] = value;
        }


        private void OnPlayTypeSelect(int index)
        {
            //加载属性
            var x = Resources.Load<CharacterProper>("Character/" + playerTypeName[index]);
            if (x == null)
            {
                GlobalMessageManager.Instance.SendTopMessage(MessageType.System,MessageLevel.Error, "没有找到该角色属性");
                Debug.LogError("没有找到该角色属性");
            }
            loadProper.Load(x);
            playTypesDescription.text = x.characterDescription;
            playTypes[playerIndex] = x.playerType;
        }

        private void OnPlayerControlSelect(int index)
        {
            // 当选择最后一个选项时（空配置）
            if (index == moveModeName.Count - 1)
            {
                // 如果当前玩家之前有选择过配置，释放该配置
                if (playerMoveMode[playerIndex] != CompareTemp && 
                    moveModeList.Contains(playerMoveMode[playerIndex]))
                {
                    int previousIndex = moveModeList.IndexOf(playerMoveMode[playerIndex]);
                    isMoveModeSelect[previousIndex] = false;
                }
        
                playerMoveDescription.text = "请选择一个键位配置";
                playerMoveMode[playerIndex] = CompareTemp;
                return;
            }
    
            // 当选择已选中的配置时，不做任何操作
            if (moveModeList[index] == playerMoveMode[playerIndex])
            {
                playerMoveDescription.text = moveModeList[index].description;
                return;
            }
    
            // 当选择新配置时
            if (!isMoveModeSelect[index])
            {
                // 如果当前玩家之前有选择过配置，释放该配置
                if (playerMoveMode[playerIndex] != CompareTemp && 
                    moveModeList.Contains(playerMoveMode[playerIndex]))
                {
                    int previousIndex = moveModeList.IndexOf(playerMoveMode[playerIndex]);
                    isMoveModeSelect[previousIndex] = false;
                }
        
                // 选择新配置
                playerMoveDescription.text = moveModeList[index].description;
                playerMoveMode[playerIndex] = moveModeList[index];
                isMoveModeSelect[index] = true;
            }
            else
            {
                // 如果配置已被其他玩家选择，回退到之前的选择
                playerMoveSelect.value = playerMoveMode[playerIndex] == CompareTemp ? 
                    moveModeName.Count - 1 : 
                    moveModeList.IndexOf(playerMoveMode[playerIndex]);
            }
        }

        
        public void OnNextBtnClick()
        {
            mapIndex++;
            mapIndex = mapIndex % mapSelectInfoList.Count;
            SetMapSelectInfo(mapIndex);
        }
        
        public void OnPrevBtnBtnClick()
        {
            mapIndex--;
            mapIndex = (mapIndex + mapSelectInfoList.Count) % mapSelectInfoList.Count;
            SetMapSelectInfo(mapIndex);
        }
        
        
        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }

        private void OnStartClick()
        {
            if (isMatching)
            {
                isMatching = false;
                startBtn.GetComponentInChildren<TextMeshProUGUI>().text = "开始匹配";
                TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameCancelMatch));
                return;
            }
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameStartMatch, new Dictionary<string, object>
            {
                {"id", mapSelectInfoList[mapIndex].mapId},
                {"career", playerTypeName[playTypesSelect.value]},
                {"controlConfig", moveModeList.IndexOf(playerMoveMode[playerIndex])}
            }));
            startBtn.GetComponentInChildren<TextMeshProUGUI>().text = "取消匹配";
            isMatching = true;
        }

    }
}