using System.Collections.Generic;
using Core.Net;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Player
{
    public class OnlinePlayerController : PlayerController
    {
        private double syncInterval = 0.1;
        private double passTime;
        
        override protected void MoveUpdate()
        {
            base.MoveUpdate();
            if (passTime < Time.deltaTime)
            {
                passTime = passTime + Time.deltaTime;
            } 
            else
            {
                passTime = passTime - Time.deltaTime;
                Vector3 playerPos = transform.position;
                Vector3 playerRotation = transform.rotation.eulerAngles;
                TcpGameClient.SendMessage(new NetMessage(CmdType.Move, new Dictionary<string, object>
                {
                    {"x", playerPos.x * 100}, //乘以100是为了避免精度问题
                    {"y", playerPos.y * 100},
                    {"z", playerPos.z * 100},
                    {"angle", playerRotation.y},
                }));
            }
        }
    }
}