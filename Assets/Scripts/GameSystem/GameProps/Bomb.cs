using System.Collections.Generic;
using Config;
using GameSystem.Character.Enemy.EnemyAI;
using GameSystem.Character.Player;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameProps.Item;
using GameSystem.GameScene;
using GameSystem.Map;
using GameSystem.Pool;
using UnityEngine;

// UnityEditor命名空间只能在编辑器脚本中使用，已移除

namespace GameSystem.GameProps
{
    public class Bomb : BaseObject
    {
        [Tooltip("创建者Id")] public string ownerId;

        [Tooltip("服务端炸弹ID（在线模式下由服务端分配，用于 BOMB_EXPLODE 匹配）")]
        public string serverBombId;

        [Tooltip("初始放置位置")] public Vector3 putPosition;

        [Tooltip("爆炸时间")] public float bombFuseTime = 3f;

        [Tooltip("炸弹伤害")] public float bombDamage = 20; //爆炸伤害

        [Tooltip("炸弹爆炸范围")] public float bombRadius = 5f;

        [Tooltip("是否爆炸")] public bool isExplode = false;

        [Tooltip("是否为在线模式炸弹（不启动本地计时器，等待服务端 BOMB_EXPLODE）")]
        public bool isOnlineBomb = false;    

        private readonly HashSet<string> hitPlayers = new HashSet<string>(); // 用于记录已经爆炸伤害过的玩家

        // private void Awake()
        // {
        //     id = gameObject.GetInstanceID().ToString();
        // }

        private void OnEnable()
        {
            print("炸弹创建成功，创建者Id：" + ownerId + "，爆炸时间：" + bombFuseTime);
            // TODO: Invoke通过统一的时间管理器触发
            Invoke("Explode", bombFuseTime);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(nameof(ObjectType.Player)) &&
                other.gameObject.GetComponent<PlayerController>()?.Id == ownerId)
            {
                //print("玩家离开炸弹范围，取消触发");
                GetComponent<Collider>().isTrigger = false;
            }
            else if (other.CompareTag(nameof(ObjectType.Enemy)) &&
                     other.gameObject.GetComponent<EnemyAIController>()?.Id == ownerId)
            {
                print("敌人离开炸弹范围，取消触发");
                GetComponent<Collider>().isTrigger = false;
            }
        }


        private void CreateExplosion(Vector3 basePos, Vector3 exportWay, List<KeyValuePair<BaseObject, TagType>> removeList, List<KeyValuePair<BaseObject, TagType>> invokeList)
        {
            print("开始执行爆炸操作,爆炸传播方向：" + exportWay);
            for (var i = 1; i < bombRadius; i++)
            {
                basePos += exportWay;
                var mapDataTarget = MapInfo.Instance.GetMapDataTarget(basePos);
                if(mapDataTarget == null) return;
                // 快照副本防止迭代时字典被事件回调修改
                var mapDataSnapshot = new List<KeyValuePair<BaseObject, TagType>>(mapDataTarget);
                foreach (var tagType in mapDataSnapshot)
                {
                    switch (tagType.Value)
                    {
                        case TagType.Bomb:
                            if (gameObject != tagType.Key.gameObject)
                            {
                                print("碰撞到其他炸弹，触发其他炸弹爆炸");
                                invokeList.Add(tagType);
                            }
                            break;
                        case TagType.Player:
                            if (hitPlayers.Contains(tagType.Key.Id))
                            {
                                print("玩家已经受到伤害，跳过");
                                break;
                            }
                            GameEventSystem.Broadcast(new CharacterTakeDamageEvent
                            {
                                Id = ownerId,
                                HitId = tagType.Key.Id,
                                Damage = bombDamage
                            });
                            hitPlayers.Add(tagType.Key.Id);
                            break;
                        case TagType.Enemy:
                            if (hitPlayers.Contains(tagType.Key.Id))
                            {
                                print("敌人已经受到伤害，跳过");
                                break;
                            }
                            GameEventSystem.Broadcast(new CharacterTakeDamageEvent
                            {
                                Id = ownerId,
                                HitId = tagType.Key.Id,
                                Damage = bombDamage
                            });
                            hitPlayers.Add(tagType.Key.Id);
                            break;
                        case TagType.Destructible:
                            GameEventSystem.Broadcast(new ExpAddEvent
                            {
                                PlayerId = ownerId,
                                Exp = 10
                            });
                            removeList.Add(tagType);
                            break;
                    }
                }
                    
                var explosionPos = basePos;
                explosionPos.y = 0f;
                ExplodePool.Instance.GetExplode(explosionPos, Quaternion.identity);
            }
        }

        public void Explode()
        {
            // 防止重复爆炸（引信计时器与连锁引爆的竞态条件）
            if (isExplode) return;
            isExplode = true;
            CancelInvoke("Explode");

            // 在线模式：本地计时器未成功取消时的安全网——仅播放视觉效果，不处理伤害
            if (isOnlineBomb)
            {
                Debug.LogWarning($"[Bomb] 在线炸弹[{serverBombId}]本地计时器触发（安全网），转由服务端处理");
                CleanupFromServer();
                return;
            }
            List<KeyValuePair<BaseObject, TagType>> removeList = new List<KeyValuePair<BaseObject, TagType>>();
            List<KeyValuePair<BaseObject, TagType>> invokeList = new List<KeyValuePair<BaseObject, TagType>>();
            
            GetComponent<Collider>().enabled = false; //关闭碰撞体，防止重复调用
            // TODO： 添加爆炸逻辑
            var bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0.5f;
            var mapDataTarget = MapInfo.Instance.GetMapDataTarget(bombPos);
            // 快照副本防止迭代时字典被事件回调修改
            var mapDataSnapshot = new List<KeyValuePair<BaseObject, TagType>>(mapDataTarget);
            foreach (var tagType in mapDataSnapshot)
            {
                switch (tagType.Value) 
                { 
                    case TagType.Bomb: 
                        if (gameObject != tagType.Key.gameObject) 
                        { 
                            print("碰撞到其他炸弹，触发其他炸弹爆炸"); 
                            invokeList.Add(tagType);
                        } 
                        break;
                    case TagType.Player: 
                        if (hitPlayers.Contains(tagType.Key.Id)) 
                        { 
                            print("玩家已经受到伤害，跳过"); 
                            break;
                        } 
                        GameEventSystem.Broadcast(new CharacterTakeDamageEvent
                        { 
                            Id = ownerId, 
                            HitId = tagType.Key.Id, 
                            Damage = bombDamage
                        }); 
                        hitPlayers.Add(tagType.Key.Id); 
                        break;
                    case TagType.Enemy: 
                        if (hitPlayers.Contains(tagType.Key.Id)) 
                        { 
                            print("敌人已经受到伤害，跳过"); 
                            break;
                        } 
                        GameEventSystem.Broadcast(new CharacterTakeDamageEvent 
                        { 
                            Id = ownerId, 
                            HitId = tagType.Key.Id, 
                            Damage = bombDamage
                        }); 
                        hitPlayers.Add(tagType.Key.Id); 
                        break;
                    case TagType.Destructible: 
                        GameEventSystem.Broadcast(new ExpAddEvent 
                        { 
                            PlayerId = ownerId, 
                            Exp = 10
                        });
                        removeList.Add(tagType);
                        break;
                    default:
                        Debug.LogError("未知的TagType: " + tagType.Value);
                        break;
                }
            }
            
            var explosionPos = bombPos;
            explosionPos.y = 0f;
            ExplodePool.Instance.GetExplode(explosionPos, Quaternion.identity);
            CreateExplosion(bombPos, Vector3.forward, removeList, invokeList);
            CreateExplosion(bombPos, Vector3.back, removeList, invokeList);
            CreateExplosion(bombPos, Vector3.left, removeList, invokeList);
            CreateExplosion(bombPos, Vector3.right, removeList, invokeList);

            // 先处理可破坏方块，确保被连锁炸弹扫描前已从地图移除，避免双重处理
            foreach (var tagType in removeList)
            {
                MapInfo.Instance.RemoveItem(tagType.Key.transform.position, tagType.Key);
                var x = tagType.Key as Destructible;
                if (x != null)
                {
                    PropsStatus propsStatus = x.CreateItem();
                    if (propsStatus != null)
                    {
                        // 将道具注册到地图系统
                        propsStatus.VirtualPosition = MapInfo.Instance.GetVirtualCoord(propsStatus.transform.position);
                        MapInfo.Instance.AddItem(propsStatus.transform.position, propsStatus, TagType.Props);
                        
                        // 设置道具外观
                        var renderers = propsStatus.gameObject.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < renderers.Length; i++)
                        {
                            var materials = renderers[i].materials;
                            for (int j = 0; j < materials.Length; j++)
                            {
                                materials[j].color = propsStatus.propsConfig.propsMaterial.color;
                            }
                
                        }

                        // 设置道具大小
                        switch (propsStatus.propsConfig.propsSize)
                        {
                            case PropsSize.Small:
                                propsStatus.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                break;
                            case PropsSize.Medium:
                                propsStatus.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                                break;
                            case PropsSize.Large:
                                propsStatus.transform.localScale = new Vector3(1f, 1f,1f);
                                break;
                            default:
                                Debug.LogError("未知的道具大小: " + propsStatus.propsConfig.propsSize);
                                break;
                        }
                        
                    }
                    DestructiblePool.Instance.ReturnDestructible(x);
                }
            }

            // 再处理连锁引爆：此时地图上的可破坏方块已清除，被连锁炸弹不会重复处理
            foreach (var tagType in invokeList)
            {
                var x = tagType.Key.GetComponent<Bomb>();
                if (!x.isExplode)
                {
                    x.Explode();
                }
            }
            
            BombPool.Instance.ReturnBomb(this);
            MapInfo.Instance.RemoveItem(transform.position, this);
            GameEventSystem.Broadcast(new BombEvents.BombDestroyEvent
            {
                Position = putPosition,
                Id = ownerId
            }); //通知爆炸事件，用于销毁爆炸
        }

        /// <summary>
        ///     在线模式：服务端 BOMB_EXPLODE 触发的纯视觉效果清理
        ///     不造成伤害、不触发事件（伤害已由服务端 HP_CHANGE 权威处理）
        /// </summary>
        public void CleanupFromServer()
        {
            if (isExplode) return;
            isExplode = true;
            CancelInvoke("Explode");

            GetComponent<Collider>().enabled = false;

            // 对齐炸弹位置到格子中心（与服务端 scanCell/scanDirection 的 gridSize=250 对应）
            var bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0f;

            // 收集爆炸范围内需要移除的可破坏方块（不生成道具，道具由服务端 PROP_SPAWN 下发）
            var removeList = new List<KeyValuePair<BaseObject, TagType>>();

            // 中心点：收集可破坏方块 + 播放爆炸效果
            CollectDestructiblesAt(bombPos, removeList);
            ExplodePool.Instance.GetExplode(bombPos, Quaternion.identity);

            // 四个方向：扫描收集可破坏方块 + 播放爆炸效果（遇墙/边界停止）
            ScanDirectionForCleanup(bombPos, Vector3.forward, removeList);
            ScanDirectionForCleanup(bombPos, Vector3.back, removeList);
            ScanDirectionForCleanup(bombPos, Vector3.left, removeList);
            ScanDirectionForCleanup(bombPos, Vector3.right, removeList);

            // 移除可破坏方块并回收（不生成道具，道具由服务端 PROP_SPAWN 单独下发）
            foreach (var tagType in removeList)
            {
                MapInfo.Instance.RemoveItem(tagType.Key.transform.position, tagType.Key);
                var dest = tagType.Key as Destructible;
                if (dest != null)
                {
                    DestructiblePool.Instance.ReturnDestructible(dest);
                }
            }

            // 回收炸弹
            BombPool.Instance.ReturnBomb(this);
            MapInfo.Instance.RemoveItem(transform.position, this);
            GameEventSystem.Broadcast(new BombEvents.BombDestroyEvent
            {
                Position = putPosition,
                Id = ownerId
            });
        }

        /// <summary>
        ///     在线模式专用：沿方向扫描，收集可破坏方块并播放爆炸视觉效果
        ///     与服务端 scanDirection 对称：逐格扫描，遇到无 MapData（墙/边界）停止
        /// </summary>
        private void ScanDirectionForCleanup(Vector3 basePos, Vector3 direction,
            List<KeyValuePair<BaseObject, TagType>> removeList)
        {
            for (var i = 1; i < bombRadius; i++)
            {
                basePos += direction;
                var mapDataTarget = MapInfo.Instance.GetMapDataTarget(basePos);
                // 无 MapData 表示遇到墙或地图边界，停止该方向传播（与服务端 isWall 对称）
                if (mapDataTarget == null) return;

                // 收集该位置的可破坏方块
                foreach (var tagType in mapDataTarget)
                {
                    if (tagType.Value == TagType.Destructible)
                    {
                        removeList.Add(tagType);
                    }
                }

                var explosionPos = basePos;
                explosionPos.y = 0f;
                ExplodePool.Instance.GetExplode(explosionPos, Quaternion.identity);
            }
        }

        /// <summary>
        ///     在线模式专用：收集指定位置的可破坏方块（中心点扫描，与服务端 scanCell 对称）
        /// </summary>
        private void CollectDestructiblesAt(Vector3 position,
            List<KeyValuePair<BaseObject, TagType>> removeList)
        {
            var mapDataTarget = MapInfo.Instance.GetMapDataTarget(position);
            if (mapDataTarget == null) return;
            foreach (var tagType in mapDataTarget)
            {
                if (tagType.Value == TagType.Destructible)
                {
                    removeList.Add(tagType);
                }
            }
        }
    }
}