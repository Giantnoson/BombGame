using GameSystem.GameScene.MainMenu.Pool;
using UnityEngine;

namespace GameSystem.GameScene.MessageScene
{
    public class GlobalMessageManager : MonoBehaviour
    {
        #region 单例模式
        public static GlobalMessageManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region 变量定义
        public GameObject topMessagePrefab;
        public GameObject leftButtonMessagePrefab;
        
        [Header("消息配置")]
        [Tooltip("默认消息等级")]
        public MessageLevel defaultMessageLevel = MessageLevel.Normal;
        [Tooltip("正常消息颜色")]
        public Color normalColor;
        [Tooltip("警告消息颜色")]
        public Color warningColor;
        [Tooltip("错误消息颜色")]
        public Color errorColor;
        #endregion

        
        #region 发送消息
        /// <summary>
        /// 发送顶部消息
        /// </summary>
        /// <param name="message">消息名称</param>
        /// <param name="duration">存续时间</param>
        public void SendTopMessage(Message message)
        {
            //当默认等级大于当前等级，则退出
            if (defaultMessageLevel > message.level)
                return;
            switch (message.level)
            {
                case MessageLevel.Normal:
                    message.BGColor = normalColor;
                    break;
                case MessageLevel.Warning:
                    message.BGColor = warningColor;
                    break;
                case MessageLevel.Error:
                    message.BGColor = errorColor;
                    break;
                default:
                    Debug.LogError("未知的消息等级");
                    return;
            }
            var x = MessagePool.Instance.GetMessage();
            x.GetComponent<Message>().SetMessage(message);
            x.gameObject.SetActive(true);
            x.transform.SetParent(topMessagePrefab.transform);
        }
        
        /// <summary>
        /// 发送顶部消息
        /// </summary>
        /// <param name="message">消息名称</param>
        /// <param name="duration">存续时间</param>
        public void SendTopMessage(MessageType type, MessageLevel level, string message, int duration = 3)
        {
            if (defaultMessageLevel > level)
                return;
            var x = MessagePool.Instance.GetMessage();
            x.gameObject.SetActive(true);
            switch (level)
            {
                case MessageLevel.Normal:
                    x.GetComponent<Message>().SetMessage(type, level, message, duration, normalColor);
                    x.transform.SetParent(topMessagePrefab.transform);
                    break;
                case MessageLevel.Warning:
                    x.GetComponent<Message>().SetMessage(type, level, message, duration, warningColor);
                    x.transform.SetParent(topMessagePrefab.transform);
                    break;
                case MessageLevel.Error:
                    x.GetComponent<Message>().SetMessage(type, level, message, duration, errorColor);
                    x.transform.SetParent(topMessagePrefab.transform);
                    break;
                default:
                    Debug.LogError("未知的消息等级");
                    MessagePool.Instance.ReturnObject(x);
                    return;
            }
        }
        
        public void SendTopMessage(string message)
        {
            SendTopMessage(MessageType.System, MessageLevel.Normal, message);
        }
        
        public void SendLeftButtonMessage(Message message)
        {
            //当默认等级大于当前等级，则退出
            if (defaultMessageLevel > message.level)
                return;
            switch (message.level)
            {
                case MessageLevel.Normal:
                    message.BGColor = normalColor;
                    break;
                case MessageLevel.Warning:
                    message.BGColor = warningColor;
                    break;
                case MessageLevel.Error:
                    message.BGColor = errorColor;
                    break;
                default:
                    Debug.LogError("未知的消息等级");
                    return;
            }
            var x = MessagePool.Instance.GetMessage();
            x.gameObject.SetActive(true);
            x.GetComponent<Message>().SetMessage(message);
            x.transform.SetParent(leftButtonMessagePrefab.transform);
        }
                
        public void SendLeftButtonMessage(MessageType type, MessageLevel level, string message, int duration = 3)
        {
            if (defaultMessageLevel > level)
                return;
            var x = MessagePool.Instance.GetMessage();
            x.gameObject.SetActive(true);
            switch (level)
            {
                case MessageLevel.Normal:
                    x.GetComponent<Message>().SetMessage(type, level, message, duration, normalColor);
                    x.transform.SetParent(leftButtonMessagePrefab.transform);
                    break;
                case MessageLevel.Warning:
                    x.GetComponent<Message>().SetMessage(type, level, message, duration, warningColor);
                    x.transform.SetParent(leftButtonMessagePrefab.transform);
                    break;
                case MessageLevel.Error:
                    x.GetComponent<Message>().SetMessage(type, level, message, duration, errorColor);
                    x.transform.SetParent(leftButtonMessagePrefab.transform);
                    break;
                default:
                    Debug.LogError("未知的消息等级");
                    MessagePool.Instance.ReturnObject(x);
                    return;
            }
        }
        #endregion
    }

    public enum MessageType
    {
        System,
        Player,
    }

    public enum MessageLevel
    {
        Normal,
        Warning,
        Error,
    }
    
    
    
}

