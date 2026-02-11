

using System;
using GameSystem.GameScene.MainMenu.Pool;
using UnityEngine;

namespace GameSystem.GameScene.MessageScene.Tests
{
    public class GlobalMessageManagerTests : MonoBehaviour
    {
        private GlobalMessageManager messageManager;
        
        [Header("消息基础信息")]
        [Tooltip("消息类型")]
        public MessageType type = MessageType.System;
        [Tooltip("消息等级")]
        public MessageLevel level = MessageLevel.Normal;
        [Tooltip("消息内容")]
        public string message = "你好啊";
        [Tooltip("消息持续时间")]
        public int duration = 20;
        
        [Header("消息位置信息")]
        [Tooltip("消息位置")]
        public bool isInTop = true;
        [Tooltip("消息通知等级")]
        public MessageLevel messageLevel;

        [Tooltip("是否打印")] public bool isPrint;


        public void Update()
        {
            if (isPrint)
            {
                PrintMessage();
                isPrint = false;
            }
        }

        private void PrintMessage()
        {
            GlobalMessageManager.Instance.defaultMessageLevel = messageLevel;
            if (isInTop)
            {
                GlobalMessageManager.Instance.SendTopMessage(type,level,message,duration);
            }
            else
            {
                GlobalMessageManager.Instance.SendLeftButtonMessage(type,level,message,duration);
            }
        }
    }
}
