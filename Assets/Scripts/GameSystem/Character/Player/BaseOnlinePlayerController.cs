using Core.Net;
using GameSystem.GameScene.MainMenu.EventSystem;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Player
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
            GameEventSystem.AddListener<BombEvents.PutBombEvent>(OnPutBomb);
            isCameraViewUpdate = true;
            // GameEventSystem.AddListener<BombEvents.BombExplodeEvent>(OnBombExplode);
        }

        public void OnDisable()
        {
            GameEventSystem.RemoveListener<BombEvents.PutBombEvent>(OnPutBomb);
            // GameEventSystem.RemoveListener<BombEvents.BombExplodeEvent>(OnBombExplode);
        }

        private void OnPutBomb(BombEvents.PutBombEvent evt)
        {
            Debug.Log("收到放置炸弹事件，玩家ID:" + evt.playerId + "位置:" + evt.Position + "当前玩家ID:" + PlayerId);
            if (evt.playerId != PlayerId) return; // 只处理当前玩家的放置炸弹事件
            bombCooldown = maxBombCooldown;
            bombCount--;
            var bombPos = evt.Position;

            print("炸弹放置位置:" + bombPos);
            GameEventSystem.Broadcast(new BombEvents.BombPlaceRequestEvent
            {
                Position = bombPos,
                Id = id,
                BombFuseTime = bombFuseTime,
                BombRadius = bombRadius,
                BombDamage = bombDamage
            });
        }
        
        // private void OnBombExplode(BombEvents.BombExplodeEvent evt)
        // {
        //     
        // }
        //

        protected override void PutBomb()
        {
            // if (bombCooldown > 0 || bombCount == 0)
            // {
            //     print("炸弹冷却或数量为0，放置失败");
            //     return;
            // }
            

            TcpGameClient.SendMessage(new NetMessage(CmdType.PutBomb));
        }
    }
}