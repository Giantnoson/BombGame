using System.Collections.Generic;
using Config;
using GameSystem.GameScene.MainMenu.Character.Enemy;
using GameSystem.GameScene.MainMenu.Character.Player;
using GameSystem.GameScene.MainMenu.EventSystem;
using GameSystem.GameScene.MainMenu.Map;
using GameSystem.GameScene.MainMenu.Pool;
using UnityEngine;
// UnityEditor命名空间只能在编辑器脚本中使用，已移除

namespace GameSystem.GameScene.MainMenu.GameProps
{
    public class Bomb : BaseObject
    {
        [Tooltip("创建者Id")] public string ownerId;

        [Tooltip("初始放置位置")] public Vector3 putPosition;

        [Tooltip("爆炸时间")] public float bombFuseTime = 3f;

        [Tooltip("炸弹伤害")] public float bombDamage = 20; //爆炸伤害

        [Tooltip("炸弹爆炸范围")] public float bombRadius = 5f;

        [Tooltip("是否爆炸")] public bool isExplode = false;    

        private readonly HashSet<string> hitPlayers = new HashSet<string>(); // 用于记录已经爆炸伤害过的玩家

        // private void Awake()
        // {
        //     id = gameObject.GetInstanceID().ToString();
        // }

        private void OnEnable()
        {
            print("炸弹创建成功，创建者Id：" + ownerId + "，爆炸时间：" + bombFuseTime);
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
                foreach (var tagType in mapDataTarget)
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
            CancelInvoke("Explode");
            isExplode = true;
            List<KeyValuePair<BaseObject, TagType>> removeList = new List<KeyValuePair<BaseObject, TagType>>();
            List<KeyValuePair<BaseObject, TagType>> invokeList = new List<KeyValuePair<BaseObject, TagType>>();
            
            GetComponent<Collider>().enabled = false; //关闭碰撞体，防止重复调用
            // TODO： 添加爆炸逻辑
            var bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0.5f;
            var mapDataTarget = MapInfo.Instance.GetMapDataTarget(bombPos);
            foreach (var tagType in mapDataTarget)
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

            foreach (var tagType in removeList)
            {
                MapInfo.Instance.RemoveItem(tagType.Key.transform.position, tagType.Key);
                DestructiblePool.Instance.ReturnDestructible(tagType.Key as Destructible);
            }


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
    }
}