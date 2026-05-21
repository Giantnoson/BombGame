using System.Collections.Generic;
using Config;
using Core.Net;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameProps.Item;
using GameSystem.GameScene.GameRuntimeScene;
using GameSystem.Map;
using UnityEngine;

namespace GameSystem.Character.Player
{
    public class BaseOnlinePlayerController : PlayerController
    {
        private string _playerId; // 其他玩家的ID
        public string PlayerId
        {
            get => _playerId;
            set => _playerId = value;
        }
        
        protected override void Awake()
        {
            isCameraViewUpdate = true;
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new(CmdType.PutBomb, msg =>
                {
                    string bombOwnerId = msg._body.GetString("id");
                    string bombId = msg._body.GetString("bombId");
                    if (bombOwnerId == PlayerId)
                    {
                        float x = msg._body.GetInt("x") / 100f;
                        float y = msg._body.GetInt("y") / 100f;
                        float z = msg._body.GetInt("z") / 100f;
                        bombCooldown = maxBombCooldown;
                        var bombPos = new Vector3(x, y, z);

                        print("炸弹放置位置:" + bombPos);
                        GameEventSystem.Broadcast(new BombEvents.BombPlaceRequestEvent
                        {
                            Position = bombPos,
                            Id = id,
                            BombFuseTime = bombFuseTime,
                            BombRadius = bombRadius,
                            BombDamage = bombDamage,
                            CallBack = a =>
                            {
                                if (a) bombCount--;
                            }
                        });

                        // 在线模式：查找刚创建的炸弹并设置服务端 bombId（用于 BOMB_EXPLODE 匹配）
                        // 对齐到格子中心，与 BombManager.OnPlaceRequest 的 Mathf.Ceil-0.5f 对齐一致
                        var alignedPos = new Vector3(
                            Mathf.Ceil(bombPos.x) - 0.5f,
                            0f,
                            Mathf.Ceil(bombPos.z) - 0.5f
                        );
                        GameOnLineRuntimeSceneManagerManager.Instance?.RegisterServerBomb(bombId, alignedPos);
                    }
                }),
                new (CmdType.Move, msg =>
                {
                    string movePlayerId = msg._body.GetString("id");
                    if (movePlayerId == PlayerId)
                    {
                        // 服务端权威位置同步：
                        // - 远程玩家：服务器广播其位置，更新本地视觉表现
                        // - 本地玩家：服务器拒绝非法位置时发回纠正，校正到合法位置
                        float x = msg._body.GetInt("x") / 100f;
                        float y = msg._body.GetInt("y") / 100f;
                        float z = msg._body.GetInt("z") / 100f;
                        float angle = msg._body.GetFloat("angle");
                        transform.position = new Vector3(x, y, z);
                        transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                }),
                // HP_CHANGE: 服务端权威伤害/治疗广播
                new (CmdType.HpChange, msg =>
                {
                    string hpPlayerId = msg._body.GetString("id");
                    if (hpPlayerId == PlayerId)
                    {
                        float newHp = msg._body.GetFloat("hp");
                        float oldHp = hp;
                        hp = newHp;
                        Debug.Log($"[HP_SYNC] 玩家[{PlayerId}] HP变化: {oldHp} → {newHp} (服务端权威)");
                        
                        // 如果血量归零则触发死亡
                        if (hp <= 0 && !isDie)
                        {
                            hp = 0;
                            isDie = true;
                            Debug.Log($"[HP_SYNC] 玩家[{PlayerId}] 服务端判定死亡");
                            GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
                            GameEventSystem.Broadcast(new CharacterDieEvent
                            {
                                AttackerID = "",  // 服务端已处理击杀者逻辑
                                DieId = id,
                                Exp = 0
                            });
                        }
                        else
                        {
                            GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
                        }
                    }
                }),
                // PlayerSync: 服务端每帧广播玩家状态（HP + 属性 + 位置）
                new (CmdType.PlayerSync, msg =>
                {
                    string syncPlayerId = msg._body.GetString("id");
                    if (syncPlayerId == PlayerId)
                    {
                        float syncedHp = msg._body.GetFloat("hp");
                        float syncedMaxHp = msg._body.GetFloat("maxHp");
                        int syncedLevel = msg._body.GetInt("level");
                        int syncedExp = msg._body.GetInt("exp");
                        float syncedMaxStamina = msg._body.GetFloat("maxStamina");
                        float sx = msg._body.GetInt("x") / 100f;
                        float sy = msg._body.GetInt("y") / 100f;
                        float sz = msg._body.GetInt("z") / 100f;

                        // 仅远程玩家校正位置（本地玩家位置由本地输入+服务端Move回显控制）
                        if (PlayerId != TcpGameClient.PlayerId)
                        {
                            transform.position = new Vector3(sx, sy, sz);
                        }

                        // 同步HP（服务端权威）
                        if (Mathf.Abs(hp - syncedHp) > 0.01f)
                        {
                            hp = syncedHp;
                        }

                        // 同步属性（服务端权威）
                        if (syncedMaxHp > 0) maxHp = syncedMaxHp;
                        if (syncedLevel > 0) level = syncedLevel;
                        if (syncedExp >= 0) exp = syncedExp;
                        if (syncedMaxStamina > 0) maxStamina = syncedMaxStamina;

                        // 反射HUD更新（HP+属性变化时）
                        GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
                    }
                })
            });
        }

        protected override void Update()
        {
            // 远程玩家不执行本地输入逻辑（Stamina、移动、镜头、地图位置更新均由网络消息驱动）
            if (isDie) return;
            
            BombUpdate();
            V2IUpdate(); // 在线模式下也需要检测道具拾取
        }

        /// <summary>
        /// 在线模式道具拾取：发送 PROP_PICKED_UP 到服务端，由服务端验证并广播效果
        /// 覆盖基类实现，不再本地应用道具效果
        /// </summary>
        protected override void CheckAndPickUpProps(Vector2Int pos)
        {
            // 仅本地玩家可触发拾取检测（远程玩家由服务端消息驱动）
            if (PlayerId != TcpGameClient.PlayerId) return;

            var items = MapInfo.Instance.GetItem(pos, TagType.Props);
            if (items == null || items.Count == 0) return;

            var manager = GameOnLineRuntimeSceneManagerManager.Instance;
            if (manager == null) return;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var propsStatus = items[i] as PropsStatus;
                if (!propsStatus || !propsStatus.propsConfig.canPickUp) continue;

                // 查找服务端下发的 itemId
                if (!manager.TryGetPropItemId(propsStatus, out var itemId)) continue;

                // 立即清除本地视觉（服务端会通过 PROP_PICKED_UP 广播确认）
                MapInfo.Instance.RemoveItem(pos, propsStatus);
                manager.UnregisterProp(itemId);
                Destroy(propsStatus.gameObject);

                // 向服务端发送拾取请求
                TcpGameClient.SendMessage(new NetMessage(CmdType.PropPickedUp, new NetDictionary
                {
                    {"itemId", itemId}
                }));

                Debug.Log($"[PropsOnline] 发送道具拾取请求: itemId={itemId}");
            }
        }

        protected override void PutBomb()
        {
            // 本地预检：避免无效网络请求（服务端也会做相同验证，此处减少不必要的往返）
            if (bombCooldown > 0 || bombCount == 0)
            {
                print("炸弹冷却或数量为0，放置失败");
                return;
            }

            // 附带位置信息（×100 避免浮点精度问题），服务端以此确定炸弹精确位置
            var pos = transform.position;
            TcpGameClient.SendMessage(new NetMessage(CmdType.PutBomb, new NetDictionary()
            {
                {"x", pos.x * 100},
                {"y", pos.y * 100},
                {"z", pos.z * 100}
            }));
        }
    }
}