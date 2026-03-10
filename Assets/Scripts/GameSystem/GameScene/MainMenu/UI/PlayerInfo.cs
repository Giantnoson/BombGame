using System.Collections.Generic;
using Core.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu
{
    /// <summary>
    /// 玩家信息类，用于管理玩家在UI上的显示和操作
    /// 继承自MonoBehaviour，使其能够作为组件附加到游戏对象上
    /// </summary>
    public class PlayerInfo : MonoBehaviour
    {
        [Header("UI界面")]
        [Tooltip("玩家名")]
        public TextMeshProUGUI playerNameText;  // 显示玩家名称的UI文本组件
        [Tooltip("玩家类型")]
        public TextMeshProUGUI playerTypeText;  // 显示玩家类型的UI文本组件
        [Tooltip("玩家状态")]
        public TextMeshProUGUI playerStatusText;  // 显示玩家状态的UI文本组件

        public Transform playerBtnTran;  // 玩家按钮的变换组件
        public Button removePlayer;      // 移除玩家按钮
        public Button transferRoomOwner;  // 转移房间拥有者按钮

        RoomPlayerInfo roomPlayerInfo;
        
        
        // 使用包装的字典类存储玩家信息
        private NetDictionary info = new();

        // 属性：获取玩家名称
        public string UName => info.GetString("uname");
        // 属性：获取玩家ID
        public string PlayerId => info.GetString("id");
        // 属性：获取玩家职业/类型
        public string Carrer => info.GetString("career");
        // 属性：获取玩家是否准备状态
        public bool IsReady => info.GetBool("isReady");
        // 房主ID字段
        public string LeaderId;
        
        /// <summary>
        /// 设置房主ID
        /// </summary>
        /// <param name="leaderid">要设置的房主ID</param>
        public void SetLeader(string leaderid)
        {
            LeaderId = leaderid;
        }

        /// <summary>
        /// 设置房间玩家信息并更新UI显示
        /// </summary>
        /// <param name="roomPlayerInfo">要设置的房间玩家信息</param>
        public void SetRoomPlayerInfo(RoomPlayerInfo roomPlayerInfo)
        {
            this.roomPlayerInfo = roomPlayerInfo;
            playerNameText.text = roomPlayerInfo.UName;
            playerTypeText.text = roomPlayerInfo.Carrer;
            playerStatusText.text = roomPlayerInfo.IsReady ? "准备" : "未准备";
            // 判断是否显示移除玩家和转移房主按钮
            // 只有当当前玩家不是目标玩家且当前玩家是房主时才显示这些按钮
            bool active = roomPlayerInfo.PlayerId != TcpGameClient.PlayerId;
            active = active && TcpGameClient.PlayerId == roomPlayerInfo.LeaderId;
            removePlayer.gameObject.SetActive(active);
            transferRoomOwner.gameObject.SetActive(active);
        }
        /// <summary>
        /// 设置标识表示占位
        /// </summary>
        public void SetRoomPlayerInfo()
        {
            playerNameText.text = "等待玩家进入";
            playerTypeText.text = "None";
            playerStatusText.text ="等待中";
            removePlayer.gameObject.SetActive(false);
            transferRoomOwner.gameObject.SetActive(false);
        }
        /// <summary>
        /// 构造函数，通过解析原始字符串信息初始化玩家数据
        /// </summary>
        /// <param name="rawInfo">包含玩家信息的原始字符串，格式为"key1:value1,key2:value2"</param>
        public void SetRoomPlayerInfo(NetDictionary rawInfo)
        {
            info = rawInfo;
            playerNameText.text = UName;
            playerTypeText.text = Carrer;
            playerStatusText.text = IsReady ? "准备" : "未准备";
            // 判断是否显示移除玩家和转移房主按钮
            // 只有当当前玩家不是目标玩家且当前玩家是房主时才显示这些按钮
            bool active = PlayerId != TcpGameClient.PlayerId;
            active = active && TcpGameClient.PlayerId == LeaderId;
            removePlayer.gameObject.SetActive(active);
            transferRoomOwner.gameObject.SetActive(active);
        }
        
        

        /// <summary>
        /// 组件初始化时注册按钮点击事件
        /// </summary>
        void Start()
        {
            removePlayer.onClick.AddListener(OnRemovePlayer);
            transferRoomOwner.onClick.AddListener(OnTransferRoomOwner);
        }
        
        /// <summary>
        /// 组件销毁时移除按钮点击事件，防止内存泄漏
        /// </summary>
        void OnDestroy()
        {
            removePlayer.onClick.RemoveListener(OnRemovePlayer);
            transferRoomOwner.onClick.RemoveListener(OnTransferRoomOwner);
        }

        /// <summary>
        /// 移除玩家按钮点击事件处理
        /// 发送踢出玩家的网络消息
        /// </summary>
        private void OnRemovePlayer()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameKickPlayer, new NetDictionary()
            {
                {"targetId", roomPlayerInfo.PlayerId},
            }));
        }

        /// <summary>
        /// 转移房主按钮点击事件处理
        /// 发送转移房主的网络消息
        /// </summary>
        private void OnTransferRoomOwner()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameLeaderChange, new NetDictionary()
            {
                {"targetId", roomPlayerInfo.PlayerId},
            }));
        }
    }
    
    /// <summary>
    /// 房间玩家信息类，用于存储和解析玩家的基本信息
    /// </summary>
    public class RoomPlayerInfo
    {
        /// <summary>
        /// 构造函数，通过解析原始字符串信息初始化玩家数据
        /// </summary>
        /// <param name="rawInfo">包含玩家信息的原始字符串，格式为"key1:value1,key2:value2"</param>
        public RoomPlayerInfo(NetDictionary rawInfo)
        {
            // 解析原始字符串，将其分割成键值对并存储到字典中
            info = rawInfo;
        }
        
        // 使用包装的字典类存储玩家信息
        private NetDictionary info = new();

        // 属性：获取玩家名称
        public string UName => info.GetString("uname");
        // 属性：获取玩家ID
        public string PlayerId => info.GetString("id");
        // 属性：获取玩家职业/类型
        public string Carrer => info.GetString("career");
        // 属性：获取玩家是否准备状态
        public bool IsReady => info.GetBool("isReady");
        // 房主ID字段
        public string LeaderId;
        
        /// <summary>
        /// 设置房主ID
        /// </summary>
        /// <param name="leaderid">要设置的房主ID</param>
        public void SetLeader(string leaderid)
        {
            LeaderId = leaderid;
        }
    }
}