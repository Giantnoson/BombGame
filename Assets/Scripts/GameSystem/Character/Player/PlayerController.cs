using Config;
using GameSystem.Character.common;
using GameSystem.EventSystem;
using GameSystem.EventSystem.Event;
using GameSystem.GameProps.Item;
using GameSystem.GameScene.MainMenu;
using GameSystem.Manager;
using GameSystem.Map;
using UnityEngine;

// 引入GameSystem命名空间，可能包含游戏相关的系统类

// 引入Unity引擎的核心命名空间

namespace GameSystem.Character.Player
{
    public class PlayerController : BaseState
    {
        # region 变量定义
        public PlayerControlConfig playerControlConfig;
        public float vertical;
        public float horizontal;
        
        public Vector3 moveDirection;
        public Vector3 beforeMoveDirection = Vector3.zero;

        public float cameraHorizontal;
        public float cameraVertical;
        public float beforeCameraHorizontal;
        public float beforeCameraVertical;
        public bool isStaminaUpdate;
        protected bool isCameraViewUpdate = false;
        [Header(" 玩家控制")]
        [Tooltip("水平移动")]
        public string sHorizontal;
        [Tooltip("垂直移动")]
        public string sVertical;
        [Tooltip("放置炸弹")]
        public KeyCode sputBomb;

        public Vector2Int beforeV2IPos;
        public Vector2Int currentV2IPos;
        
        #endregion

        protected virtual void Awake()
        {
            beforeV2IPos = MapInfo.Instance.GetVirtualCoord(transform.position);
            currentV2IPos = new Vector2Int(beforeV2IPos.x, beforeV2IPos.y);
            isCameraViewUpdate = GameModeSelect.PlayerCount == 1;
        }

        protected virtual void Update()
        {
            if (isDie) return; //如果玩家死亡，则不执行以下代码
            if (Input.GetKeyDown(sputBomb)) PutBomb();
            StaminaUpdate();
            BombUpdate();
            MoveUpdate();
            CameraViewUpdate();
            V2IUpdate();
        }

        protected void CameraViewUpdate()
        {
            if(!isCameraViewUpdate) return;//在离线PVP模式下不更新摄像头
            cameraHorizontal = Input.GetAxis("Mouse X");
            cameraVertical = Input.GetAxis("Mouse Y");
            if (cameraVertical == beforeCameraVertical && cameraHorizontal == beforeCameraHorizontal) return;
            GameEventSystem.Broadcast(
                new CharacterMoveEvent.UpdateRotationXEvent(id, cameraHorizontal, cameraVertical));
            beforeCameraHorizontal = cameraHorizontal;
            beforeCameraVertical = cameraVertical;
        }

        protected virtual void MoveUpdate()
        {
            horizontal = Input.GetAxis(sHorizontal); // A/D
            vertical = Input.GetAxis(sVertical); // W/S 
            moveDirection = new Vector3(horizontal, 0, vertical);

            // 将移动向量从角色本地坐标系转换到世界坐标系
            moveDirection = transform.TransformDirection(moveDirection);
            if (moveDirection == beforeMoveDirection) return;
            GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateMoveDirectionEvent(id, moveDirection));
            beforeMoveDirection = moveDirection;
        }

        protected virtual void V2IUpdate()
        {
            currentV2IPos = MapInfo.Instance.GetVirtualCoord(transform.position);
            if (beforeV2IPos != currentV2IPos)
            {
                // 旧位置无效时（首次更新 Vector2Int.zero 通常不在 MapData 中），直接添加到新位置
                if (!MapInfo.Instance.IsValidPosition(beforeV2IPos))
                {
                    if (MapInfo.Instance.IsValidPosition(currentV2IPos))
                        MapInfo.Instance.AddItem(currentV2IPos, this, TagType.Player);
                }
                else
                {
                    MapInfo.Instance.UpdateItem(currentV2IPos, beforeV2IPos, this, TagType.Player);
                }
                beforeV2IPos = currentV2IPos;
                // 检查当前区块是否有道具
                CheckAndPickUpProps(currentV2IPos);
            }
        }

        protected virtual void CheckAndPickUpProps(Vector2Int pos)
        {
            var items = MapInfo.Instance.GetItem(pos, TagType.Props);
            if (items == null || items.Count == 0) return;
            
            // 拾取所有当前区块的道具
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var propsStatus = items[i] as PropsStatus;
                if (!propsStatus || !propsStatus.propsConfig.canPickUp) continue;
                
                // 从地图系统移除道具
                MapInfo.Instance.RemoveItem(pos, propsStatus);
                
                // 初始化并使用道具（UseProps 内部会创建 Timer 管理生命周期）
                propsStatus.InitProps(id, propsStatus.propsConfig);
                propsStatus.UseProps();
                
                // 隐藏道具视觉（不销毁，因为 PropsStatus 需要保持存活以处理 Timer 回调）
                // PropsStatus 的 Timer 到期后会自行调用 PropsDisable → Destroy(gameObject)
            }
        }



        protected virtual void PutBomb()
        {
            if (bombCooldown > 0 || bombCount == 0)
            {
                print("炸弹冷却或数量为0，放置失败");
                return;
            }

            bombCooldown = maxBombCooldown;
            var bombPos = transform.position;

            print("炸弹放置位置:" + bombPos);
            GameEventSystem.Broadcast(new BombEvents.BombPlaceRequestEvent
            {
                Position = bombPos,
                Id = id,
                BombFuseTime = bombFuseTime,
                BombRadius = bombRadius,
                BombDamage = bombDamage,
                CallBack = x =>
                {
                    if (x) bombCount--;
                }
            });
        }




        /*
        private void Start()
        {

        }
        */


        #region 初始化函数

        public void PlayerControllerInit(string name, string id, CharacterType type,PlayerControlConfig playerControlConfig)
        {
            characterName = name;
            this.id = id;
            characterType = type;
            if (playerControlConfig != null)
            {
                this.playerControlConfig = playerControlConfig;
                sHorizontal = playerControlConfig.moveHorizontal;
                sVertical = playerControlConfig.moveVertical;
                sputBomb = playerControlConfig.putBomb;
            }
            //严格按照此顺序初始化
            StateInit();
            
            print("玩家属性初始化成功");
        }

        public void DisableCamera()
        {
            var cameras = GetComponentsInChildren<Camera>();
            foreach (var c in cameras)
            {
                c.gameObject.SetActive(false);
            }
        }

        protected override void OnPlayerDie(CharacterDieEvent evt)
        {
            if (isDie) return;
            if (evt.DieId == id)
            {
                isDie = true;
                id = "Die";
                GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateMoveDirectionEvent(id, Vector3.zero));
                Die();
            }
        }

        protected override void OnTakeDamage(CharacterTakeDamageEvent evt)
        {
            if (isDie) return;
            if (evt.HitId == id)
            {
                hp -= evt.Damage;
                print($"{evt.HitId} 受到来自 {evt.Id}的 {evt.Damage} 伤害。剩余血量为: {hp}");
                if (hp <= 0) //当玩家死亡时
                {
                    hp = 0;
                    print(id + "玩家死亡");
                    GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
                    GameEventSystem.Broadcast(new CharacterDieEvent
                    {
                        AttackerID = evt.Id,
                        DieId = id,
                        Exp = 50 * level
                    });
                    return;
                }

                GameEventSystem.Broadcast(new HUDEvent.TakeDamageEvent(id, hp, maxHp));
            }
        }

        #endregion
    }
}