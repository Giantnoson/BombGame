using System.Collections.Generic;
using System.Linq;
using Config;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    public class MainUIPlaySetPanel : UIBasePanel
    {
        public static int MaxCharacterCount = 4;
        [Header("基础设置")]
        [Tooltip("面板名称")]
        public string panelName = "PlaySetPanel";
        public Button nextBtn;
        public Button prevBtn;
        public Button startBtn;
        public Button backButton;
        public Button ResetBtn;
        
        
        
        
        [Tooltip("属性面板加载器")]
        public LoadProper loadProper;
        [Tooltip("玩家头部信息显示")]
        public TextMeshProUGUI playerHeadInfoText;
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
        
        [Header("角色类型设置")]
        [Tooltip("角色类型名称")]
        public List<string> playerTypeName = new List<string>();
        [Tooltip("角色类型下拉框")]
        public TMP_Dropdown playTypesSelect;
        [Tooltip("角色类型描述")]
        public TextMeshProUGUI playTypesDescription;
        
        [Header("角色名称设置")]
        [Tooltip("角色名称输入框")]
        public TMP_InputField playerNamesInput;
        
        [Header("角色ID设置")]
        [Tooltip("角色ID输入框")]
        public TextMeshProUGUI playerIdText;
        
        
        [Header("角色基础信息")]
        [Tooltip("角色类型列表")]
        public List<CharacterType> playTypes = new List<CharacterType>();
        [Tooltip("角色名称列表")]
        public List<string> playerNames = new List<string>(MaxCharacterCount);
        [Tooltip("角色ID列表")]
        public List<string> playerIds = new List<string>(MaxCharacterCount);
        [Tooltip("角色键位信息列表")]
        public List<PlayerControlConfig> playerMoveMode = new List<PlayerControlConfig>(MaxCharacterCount);
        [Tooltip("角色基础信息列表")]
        public List<CharacterBaseInfo> characterBaseInfos = new List<CharacterBaseInfo>(MaxCharacterCount);
        [Tooltip("当前索引")]
        public int playerIndex = 0;
        public int playerCount = 0;
        public int enemyCount = 0;


        public void RefreshPlayerInfos()
        {
            //角色键位刷新
            if (playerMoveMode[playerIndex].putBomb == KeyCode.None)
            {
                playerMoveDescription.text = "请选择一个键位配置";
                playerMoveSelect.value = moveModeName.Count - 1;
            }
            else
            {
                playerMoveDescription.text = playerMoveMode[playerIndex].description;
                playerMoveSelect.value = moveModeList.FindIndex(index => index == playerMoveMode[playerIndex]);
            }
            
            //角色类型刷新
            var x = Resources.Load<CharacterProper>("Character/" + playTypes[playerIndex].ToString());
            if (x == null)
            {
                Debug.LogError("没有找到该角色属性");
            }
            loadProper.Load(x);
            playTypesDescription.text = x.characterDescription;
            playTypesSelect.value = playerTypeName.FindIndex(index => index == playTypes[playerIndex].ToString());
            
            //初始化序列显示
            playerHeadInfoText.text = playerHeadStr[playerIndex];
            
            //角色ID刷新
            playerIdText.text = playerIds[playerIndex];
            
            //角色名称刷新
            playerNamesInput.text = playerNames[playerIndex];

        }
        
        private void OnEnable()
        {
            playerCount = GameModeSelect.PlayerCount;
            enemyCount = GameModeSelect.EnemyCount;
            //初始化角色ID
            var x = Resources.Load<PlayerControlList>("PlayerControl/PlayerControlList");
            if (x == null)
            {
                Debug.LogError("没有找到该角色键位列表");
            }
            moveModeList = x.playerMoveModeConfigs;
            for (int i = 0; i < playerCount; i++)
            {
                playTypes.Add(CharacterType.Balance);
                playerNames.Add($"Player{i}");
                playerIds.Add($"P{i + 1}");
                playerMoveMode.Add(new PlayerControlConfig());
                playerHeadStr.Add("玩家" + i);
            }
            for (int i = playerCount; i < playerCount + enemyCount; i++)
            {
                playTypes.Add(CharacterType.Enemy);
                playerNames.Add($"Enemy{(i - playerCount) + 1}");
                playerIds.Add($"E{i - playerCount + 1}");
                playerMoveMode.Add(new PlayerControlConfig());
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
            playerMoveSelect.value = moveModeName.Count - 1;
            playerNamesInput.text = playerNames[playerIndex];
            
            playerNamesInput.onValueChanged.AddListener(OnPlayerNameInput);
            nextBtn.onClick.AddListener(OnNextBtnClick);
            prevBtn.onClick.AddListener(OnPrevBtnBtnClick);
            startBtn.onClick.AddListener(OnStartClick);
            backButton.onClick.AddListener(OnBackClick);
            ResetBtn.onClick.AddListener(OnResetClick);
            
            
            RefreshPlayerInfos();
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
                Debug.LogError("没有找到该角色属性");
            }
            loadProper.Load(x);
            playTypesDescription.text = x.characterDescription;
            playTypes[playerIndex] = x.playerType;
        }

        private void OnPlayerControlSelect(int index)
        {
            if (index == moveModeName.Count - 1)
            {
                playerMoveDescription.text = "请选择一个键位配置";
                playerMoveMode[playerIndex] = new PlayerControlConfig();
                
            }
            else
            {
                playerMoveDescription.text = moveModeList[index].description;
                playerMoveMode[playerIndex] = moveModeList[index];     
            }

        }
        
        public void OnNextBtnClick()
        {
            playerIndex++;
            playerIndex %= playerCount;
            RefreshPlayerInfos();
        }
        
        public void OnPrevBtnBtnClick()
        {
            playerIndex--;
            playerIndex = (playerIndex + playerCount) % playerCount;
            RefreshPlayerInfos();
        }
        
        
        private void OnBackClick()
        {
            MainUIManager.Instance.Back();
        }

        private void OnStartClick()
        {

            // 使用GameFlowManager.StartSinglePlayerGame方法，按照正常流程加载游戏
            if (GameFlowManager.Instance == null || GameFlowManager.Instance == null)
            {
                Debug.LogError("没有找到GameFlowManager或GameFlowManager");
                return;
            }
            
            
            for (int i = 0; i < playerCount; i++)
            {
                characterBaseInfos.Add(new CharacterBaseInfo(playTypes[i], playerNames[i], playerIds[i], playerMoveMode[i]));
            }

            for (int i = playerCount; i < enemyCount + playerCount; i++)
            {
                characterBaseInfos.Add(new CharacterBaseInfo(playTypes[i], playerNames[i], playerIds[i], null));
            }
            GameModeSelect.CharacterBaseInfos = characterBaseInfos;
            
            GameModeSelect.Instance.StartGame();
            MainUIManager.Instance.CloseAll();
        }

        private void OnResetClick()
        { 
            playerCount = GameModeSelect.PlayerCount;
            enemyCount = GameModeSelect.EnemyCount;
            //初始化角色ID
            var x = Resources.Load<PlayerControlList>("PlayerControl/PlayerControlList");
            if (x == null)
            {
                Debug.LogError("没有找到该角色键位列表");
            }
            moveModeList = x.playerMoveModeConfigs;
            for (int i = 0; i < playerCount; i++)
            {
                playTypes.Add(CharacterType.Balance);
                playerNames.Add($"Player{i}");
                playerIds.Add($"P{i + 1}");
                playerMoveMode.Add(new PlayerControlConfig());
                playerHeadStr.Add("玩家" + i);
            }
            for (int i = playerCount; i < playerCount + enemyCount; i++)
            {
                playTypes.Add(CharacterType.Enemy);
                playerNames.Add($"Enemy{(i - playerCount) + 1}");
                playerIds.Add($"E{i - playerCount + 1}");
                playerMoveMode.Add(new PlayerControlConfig());
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
            playerMoveSelect.value = moveModeName.Count - 1;
            playerNamesInput.text = playerNames[playerIndex];
            RefreshPlayerInfos();
        }
    }
}