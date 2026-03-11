using System.Collections.Generic;
using Core.Net;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Character.Player
{
/**
 * 在线玩家控制器类，继承自基础在线玩家控制器
 * 用于处理网络游戏中玩家的移动、同步和死亡等行为
 */
    public class OnlinePlayerController : BaseOnlinePlayerController
    {
        // 同步间隔时间，单位为秒
        private double syncInterval = 1/30.0;
        // 经过的时间计数器
        private double passTime = 0;

        private Vector3 lastPos;

        private Vector3 lastRot;
        
        /**
         * 移动更新方法，重写自基类
         * 用于处理玩家移动和位置同步逻辑
         */
        override protected void MoveUpdate()
        {
            // 调用基类的移动更新方法
            base.MoveUpdate();
            // 累计经过的时间
            if (passTime < syncInterval)
            {
                passTime += Time.deltaTime;
            } 
            else
            {
                // 当达到同步间隔时，发送位置信息
                passTime = 0;
                // 获取玩家当前位置
                Vector3 playerPos = transform.position;
                // 获取玩家当前旋转角度
                Vector3 playerRotation = transform.rotation.eulerAngles;

                // 检查当前位置和旋转角度是否发生变化
                if (lastPos != playerPos || lastRot != playerRotation)
                {
                    lastPos = playerPos;
                    lastRot = playerRotation;
                    // 创建并发送移动消息到服务器
                    TcpGameClient.SendMessage(new NetMessage(CmdType.Move, new NetDictionary()
                    {
                        {"x", playerPos.x * 100}, //乘以100是为了避免精度问题
                        {"y", playerPos.y * 100},
                        {"z", playerPos.z * 100},
                        {"angle", playerRotation.y}, // 只同步Y轴旋转角度
                    }));
                }
            }
        }
        /**
         * 玩家死亡处理方法
         * 处理玩家死亡后的相关逻辑
         */
        protected override void Die()
        {
        // 调用基类的死亡处理方法
            base.Die();
        // 移除角色的控制器组件
            Destroy(gameObject.GetComponent<CharacterController>());
        }
    }
}