using System.Collections.Generic;
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
            isCameraViewUpdate = true;
            TcpGameClient.RegisterMessageHandler(this, new List<DefaultHandler>
            {
                new(CmdType.PutBomb, msg =>
                {
                    string playerId = msg._body.GetString("id");
                    if (playerId == PlayerId)
                    {
                        float x = msg._body.GetInt("x") / 100f;
                        float y = msg._body.GetInt("y") / 100f;
                        float z = msg._body.GetInt("z") / 100f;
                        bombCooldown = maxBombCooldown;
                        bombCount--;
                        var bombPos = new Vector3(x, y, z);

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
                }),
                new (CmdType.Move, msg =>
                {
                    string movePlayerId = msg._body.GetString("id");
                    if (movePlayerId == PlayerId)
                    {
                        float x = msg._body.GetInt("x") / 100f;
                        float y = msg._body.GetInt("y") / 100f;
                        float z = msg._body.GetInt("z") / 100f;
                        float angle = msg._body.GetFloat("angle");
                        transform.position = new Vector3(x, y, z); // 更新位置
                        transform.rotation = Quaternion.Euler(0, angle, 0); // 更新旋转，假设只需要更新Y轴旋转
                    }
                })
            });
        }

        protected override void PutBomb()
        {
            TcpGameClient.SendMessage(new NetMessage(CmdType.PutBomb));
        }
    }
}