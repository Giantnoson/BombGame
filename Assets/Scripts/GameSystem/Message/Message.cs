using System;
using GameSystem.GameScene.MainMenu.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.GameScene.MessageScene
{
    public class Message : MonoBehaviour
    {
        [Header("消息基础信息")]
        [Tooltip("消息类型")]
        public MessageType type;
        [Tooltip("消息等级")]
        public MessageLevel level;
        [Tooltip("消息内容")]
        public string message;
        [Tooltip("消息持续时间")]
        public int duration;
        [Tooltip("消息背景颜色")]
        public Color BGColor;
        
        [Header("消息实体")]
        [Tooltip("消息文本")]
        public TextMeshProUGUI messageText;
        [Tooltip("消息背景")]
        public Image BGImage;

        private void Awake()
        {
            if (BGImage == null || messageText == null)
            {
                Debug.LogError("Message组件未正确赋值");
            }
        }

        public void SetMessage(MessageType type, MessageLevel level, string message, int duration, Color BGColor)
        {
            this.type = type;
            this.level = level;
            this.message = message;
            this.duration = duration;
            this.BGColor = BGColor;
            messageText.text = $"{type.ToString()} - {message}";
            BGImage.color = BGColor;
            Invoke(nameof(ReturnMessage), duration);
        }

        public void SetMessage(Message message)
        {
            SetMessage(message.type, message.level, message.message, message.duration, message.BGColor);
        }
        
        

        private void ReturnMessage()
        {
            MessagePool.Instance.ReturnObject(this);
        }
    }
}