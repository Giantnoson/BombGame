using System;
using GameSystem;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace player
{
    public class PlayerController : MonoBehaviour
    {
        
        
        [Header("基础模型")]
        [Tooltip("角色类型")]
        public PlayType playerType;
        [Tooltip("角色基础属性配置")]
        public PlayerProper playerProper;
        [Header("全局属性")]
        public GlobalProper globalProper;
        [Tooltip("炸弹模型")]
        public GameObject bomb;
        [Tooltip("玩家ID")]
        public int playerId;


        [Header("属性设置")]
        [Tooltip("生命值")]
        public float hp;
        [Tooltip("体力")]
        public float stamina = 100f;
        [Tooltip("炸弹数量")]
        public int bombCount = 0;
        [Tooltip("等级")]
        public int level = 1;
        [Tooltip("经验值")]
        public int exp = 0;
        [Tooltip("当前速度")]
        public float currentSpeed;
        
        
        

        private void Start()
        {
            InitProper();
            LoadProper();
            if (bomb == null)
            {
                Debug.LogError("BombPrefab is null");
            }
        }
        
        private void LoadProper()
        {
            hp = playerProper.maxHp; // 初始化生命值
            currentSpeed = playerProper.speed; // 初始化速度
            bombCount = globalProper.initBombCount; // 初始化炸弹数量
            level = globalProper.initLevel;// 初始化等级
            exp = globalProper.initExp;// 初始化经验值
        }
        private void InitProper()
        {
            switch (playerType) {
                case PlayType.Balance:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.Balance));
                    break;
                case PlayType.Speed:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.Speed));
                    break;
                case PlayType.BombTruck:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.BombTruck));
                    break;
                case PlayType.Tank:
                    playerProper = Resources.Load<PlayerProper>(nameof(PlayType.Tank));
                    break;
                default:
                    Debug.LogError("Invalid player type");
                    break;
            }
            if (playerProper == null)
                Debug.LogError("NotFound PlayerProper");
            print("playerProper load success");
            globalProper = Resources.Load<GlobalProper>("GlobalProper");
            if (globalProper == null)
                Debug.LogError("NotFound GlobalProper");
        }

        private void Update()
        { 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PutBomb();
            }
        }
        private void PutBomb()
        {
            Vector3 bombPos = transform.position;
            bombPos.x = Mathf.Ceil(bombPos.x) - 0.5f;
            bombPos.z = Mathf.Ceil(bombPos.z) - 0.5f;
            bombPos.y = 0f;
            print("PutBomb:" + bombPos);
            EventSystem.Broadcast(new BombPlaceRequestEvent 
            { 
                position = bombPos, 
                bombPrefab = bomb,
                ownerID = playerId 
            });
        }
    }
}