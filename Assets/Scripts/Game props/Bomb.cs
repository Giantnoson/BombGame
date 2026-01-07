using System;
using System.Collections.Generic;
using GameSystem;
using player;
using UnityEditor;
using UnityEngine;

namespace Game_props
{
    public class Bomb : MonoBehaviour
    {
        [Tooltip("创建者Id")]
        public int ownerId;
        [Tooltip("初始放置位置")]
        public Vector3 putPosition;
        [Tooltip("爆炸时间")]
        public float bombFuseTime = 3f;
        [Tooltip("炸弹伤害")]
        public float bombDamage = 20; //爆炸伤害
        [Tooltip("炸弹爆炸范围")]
        public float bombRadius = 5f;
        [Tooltip("爆炸效果")]
        public GameObject explosionEffect;
        
        
        private HashSet<int> hitPlayers = new HashSet<int>();// 用于记录已经爆炸伤害过的玩家


        private void Start()
        {
            if (explosionEffect == null)
            {
                Debug.LogError("explosionEffect为空，请添加explosionEffect");
            }
            else
            {
                print("explosionEffect加载成功");
                if (!PrefabUtility.IsPartOfAnyPrefab(explosionEffect))
                {
                    Debug.LogWarning("explosionEffect字段不是预制体引用，请检查设置");
                }
            }
            print("炸弹创建成功，创建者Id：" + ownerId + "，爆炸时间：" + bombFuseTime);
            Invoke("Explode", bombFuseTime);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.gameObject.GetComponent<PlayerController>().playerId == ownerId)
            {
                GetComponentInChildren<Collider>().isTrigger = false;
            }
                
        }
        

        private void CreateExplosion(Vector3 basePos , Vector3 exportWay)
        {
            print("开始执行爆炸操作,爆炸传播方向：" + exportWay);
            for (int i = 1; i < bombRadius; i++)
            {
                basePos += exportWay;
                Collider[] hitColliders = Physics.OverlapBox(basePos, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity);
                foreach (var hitCollider in hitColliders)
                {
                    print("碰撞到 tag = " + hitCollider.tag + " name = " + hitCollider.name + " pos= " + hitCollider.transform.position);
                    if (hitCollider.CompareTag("Player"))
                    {
                        PlayerController playerController = hitCollider.gameObject.GetComponent<PlayerController>();
                        if (hitPlayers.Contains(playerController.playerId))
                        {
                           print("玩家已经受到伤害，跳过");
                            continue;
                        }
                        EventSystem.Broadcast(new PlayerTakeDamageEvent()
                        {
                            OwnerId = ownerId,
                            HitId = hitCollider.gameObject.GetComponent<PlayerController>().playerId,
                            Damage = bombDamage
                        });
                    }else if (hitCollider.CompareTag("Destructible"))
                    {
                        EventSystem.Broadcast(new ExpAddEvent()
                        {
                            PlayerId = ownerId,
                            Exp = 10
                        });
                        Destroy(hitCollider.gameObject);
                    }else if (hitCollider.CompareTag("Wall"))
                    {

                        print("碰撞到墙体，退出");
                        return;
                    }
                    else if (hitCollider.CompareTag("Bomb"))
                    {
                        if (hitCollider.gameObject != gameObject)
                        {
                            hitCollider.gameObject.GetComponent<Bomb>().Explode();
                        }
                    }

                }
                Vector3 explosionPos = basePos;
                explosionPos.y = 0f;
                Instantiate(explosionEffect, explosionPos, Quaternion.identity);
            }
        }

        public void Explode()
        {   
            GetComponent<Collider>().enabled = false;//关闭碰撞体，防止重复调用
            // TODO： 添加爆炸逻辑
            Vector3 bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0.5f;
            Collider[] hitColliders = Physics.OverlapBox(bombPos, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity);
            foreach (var hitCollider in hitColliders)
            {
                print("碰撞到 tag = " + hitCollider.tag + " name = " + hitCollider.name + " pos= " + hitCollider.transform.position);
                if (hitCollider.CompareTag("Player"))
                {
                    PlayerController playerController = hitCollider.gameObject.GetComponent<PlayerController>();
                    if (hitPlayers.Contains(playerController.playerId))
                    {
                        print("玩家已经受到伤害，跳过");
                        continue;
                    }
                    EventSystem.Broadcast(new PlayerTakeDamageEvent()
                    {
                        OwnerId = ownerId,
                        HitId = hitCollider.gameObject.GetComponent<PlayerController>().playerId,
                        Damage = bombDamage
                    });
                }
                else if (hitCollider.CompareTag("Destructible"))
                {
                    EventSystem.Broadcast(new ExpAddEvent()
                    {
                        PlayerId = ownerId,
                        Exp = 10
                    });
                    Destroy(hitCollider.gameObject);
                }
                else if (hitCollider.CompareTag("Wall"))
                {

                    print("碰撞到墙体，退出");
                    return;
                }
                else if (hitCollider.CompareTag("Bomb"))
                {
                    if (hitCollider.gameObject != gameObject)
                    {
                        hitCollider.gameObject.GetComponent<Bomb>().Explode();
                    }
                }

            }
            
            Vector3 explosionPos = bombPos;
            explosionPos.y = 0f;
            Instantiate(explosionEffect, explosionPos, Quaternion.identity);
            CreateExplosion(bombPos, Vector3.forward);
            CreateExplosion(bombPos, Vector3.back);
            CreateExplosion(bombPos, Vector3.left);
            CreateExplosion(bombPos, Vector3.right);
            EventSystem.Broadcast(new BombDestroyEvent
            {
                position = putPosition,
                ownerId = ownerId
            });//通知爆炸事件，用于销毁爆炸
            Destroy(gameObject);
        }
        
        
    }
}