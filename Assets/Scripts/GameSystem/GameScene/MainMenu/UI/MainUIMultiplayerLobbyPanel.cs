using System.Collections;
using System.Collections.Generic;
using Config;
using Core.Net;
using GameSystem.Message;
using GameSystem.Pool;
using GameSystem.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MainMenu.UI
{
    public class MainUIMultiplayerLobbyPanel : UIBasePanel
    {
        public override PanelSymbol symbol => PanelSymbols.MultiPlayerLobbyPanel;
        public Button createRoomBtn;
        public Button joinRoomBtn;
        public Button matchingBtn;
        public Button backBtn;
        public GameObject roomListParent;
        public GameObject roomListPrefab;
        public TMP_InputField roomNameInput;

        /// <summary>对象池（场景中挂载 RoomListInfoPool 组件）</summary>
        private RoomListInfoPool _pool;

        /// <summary>当前显示的条目列表（用于归还）</summary>
        private readonly List<RoomListInfo> _displayedEntries = new List<RoomListInfo>();

        /// <summary>缓存的 MapSelectInfoList，避免每次 SetRoomInfo 都 Resources.Load</summary>
        private MapSelectInfoList _cachedMapList;

        /// <summary>自动刷新房间列表的协程引用，面板 Hide 时停止</summary>
        private Coroutine _autoRefreshRoutine;

        /// <summary>自动刷新间隔（秒）</summary>
        private const float AutoRefreshInterval = 5f;

        private void RegisterMessageHandler()
        {
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new (CmdType.BaseGameReqRoomInfo, msg =>
                {
                    var rooms = msg._body.GetDictionary("rooms");
                    UpdateRoomList(rooms);
                }),
                new (CmdType.BaseGameJoinRoom, msg =>
                {
                    string result = msg._body.GetString("result");
                    if (result == "success")
                    {
                        string roomInfo = msg._body.GetString("info");
                        MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerRoomPanel, parameters: new Dictionary<string, string>
                        {
                            {"info", roomInfo}
                        });
                        Hide();
                    }
                    else
                    {
                        GlobalMessageManager.Instance.SendTopMessage($"加入房间失败: {msg._body.GetString("reason")}");
                    }
                })
            });

        }

        /// <summary>
        /// 基于对象池更新房间列表：全部归还旧条目 → 从池中获取新条目 → 更新数据
        /// </summary>
        private void UpdateRoomList(NetDictionary rooms)
        {
            // 确保对象池已初始化
            if (!EnsurePoolReady()) return;

            // 延迟加载缓存
            if (_cachedMapList == null)
                _cachedMapList = Resources.Load<MapSelectInfoList>(MapSelectInfoList.OnLineConfig);

            // 归还当前显示的所有条目到池中
            for (int i = 0; i < _displayedEntries.Count; i++)
            {
                _pool.ReturnEntry(_displayedEntries[i]);
            }
            _displayedEntries.Clear();

            // 无房间数据时提前返回
            if (rooms == null) return;

            // 从池中获取条目并更新数据
            foreach (NetDictionary rawInfo in rooms.Values)
            {
                var entry = _pool.GetEntry(roomListParent.transform);
                if (entry == null)
                {
                    Debug.LogWarning("[LobbyPanel] 对象池已满，无法获取更多 RoomListInfo");
                    break;
                }

                var info = new RoomInfo(rawInfo);
                entry.SetRoomInfo(info, _cachedMapList);
                _displayedEntries.Add(entry);
            }
        }

        /// <summary>
        /// 确保对象池可用：优先查找场景中的 RoomListInfoPool.Instance，
        /// 若不存在则从 roomListPrefab 创建并设置
        /// </summary>
        private bool EnsurePoolReady()
        {
            if (_pool != null) return true;

            _pool = RoomListInfoPool.Instance;
            if (_pool != null)
            {
                // 如果池没有 prefab，尝试从本面板的引用设置
                if (_pool.prefab == null && roomListPrefab != null)
                {
                    var component = roomListPrefab.GetComponent<RoomListInfo>();
                    if (component != null)
                        _pool.prefab = component;
                    else
                    {
                        Debug.LogError("[LobbyPanel] roomListPrefab 上没有 RoomListInfo 组件");
                        GlobalMessageManager.Instance.SendTopMessage(MessageType.System, MessageLevel.Error, "房间列表预制体配置错误");
                    }
                }
                return _pool.prefab != null;
            }

            Debug.LogError("[LobbyPanel] 场景中未找到 RoomListInfoPool，请在场景中添加该组件并设置 prefab");
            GlobalMessageManager.Instance.SendTopMessage(MessageType.System, MessageLevel.Error, "房间列表对象池未初始化");
            return false;
        }


        public override void Show()
        {
            base.Show();
            RegisterMessageHandler();
            RefreshRoomList();
            createRoomBtn.onClick.AddListener(OnCreateRoomClick);
            joinRoomBtn.onClick.AddListener(OnRefreshListClick);
            backBtn.onClick.AddListener(OnBackClick);
            matchingBtn.onClick.AddListener(OnMatchingClick);

            // 启动自动刷新定时器，每隔 AutoRefreshInterval 秒请求最新房间列表
            _autoRefreshRoutine = StartCoroutine(AutoRefreshCoroutine());
        }
        
        public override void Hide()
        {
            base.Hide();

            // 停止自动刷新定时器
            if (_autoRefreshRoutine != null)
            {
                StopCoroutine(_autoRefreshRoutine);
                _autoRefreshRoutine = null;
            }

            // 归还所有显示的条目到对象池
            for (int i = 0; i < _displayedEntries.Count; i++)
            {
                _pool?.ReturnEntry(_displayedEntries[i]);
            }
            _displayedEntries.Clear();

            createRoomBtn.onClick.RemoveListener(OnCreateRoomClick);
            joinRoomBtn.onClick.RemoveListener(OnRefreshListClick);
            backBtn.onClick.RemoveListener(OnBackClick);
            matchingBtn.onClick.RemoveListener(OnMatchingClick);
            GetComponent<AutoRegister>()?.UnregisterAll();
        }

        private void OnCreateRoomClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerPlaySetPanel,true);
        }

        private void OnMatchingClick()
        {
            MainUIManager.Instance.ShowPanel(PanelSymbols.MultiPlayerRandomFitPanel);
        }
        
        private void OnRefreshListClick()
        {
            RefreshRoomList();
        }

        private void RefreshRoomList()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.BaseGameReqRoomInfo));
        }

        /// <summary>
        /// 自动刷新房间列表的协程：每隔 AutoRefreshInterval 秒发送一次房间信息请求，
        /// 服务器仅在被请求时才响应（被动同步模式）。
        /// </summary>
        private System.Collections.IEnumerator AutoRefreshCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(AutoRefreshInterval);
                RefreshRoomList();
            }
        }

        private void OnBackClick()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.Logout));
            MainUIManager.Instance.Back();
        }
    }
}
