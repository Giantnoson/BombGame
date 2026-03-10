using System.Collections.Generic;
using Core.Events;

namespace Core.Net
{
    /**
     * 抽象TCP客户端类，继承自事件分发器
     * 提供了TCP客户端的基本功能和事件处理机制
     */
    public abstract class AbsTcpClient : EventDispatcher
    {
        // IP地址字段
        public string _ip;
        // 端口号字段
        public int _port;

        // 事件队列，用于存储待处理的事件
        protected Queue<Event> _eventQueues;
        // 消息列表，用于存储消息ID和对应的处理方式
        public Dictionary<int, int> msgList = new Dictionary<int, int>();

        /**
         * 初始化方法
         * 清空所有队列和数据
         */
        public void Init()
        {
            Clear();
        }

        /**
         * 清空方法
         * 由子类实现具体的清空逻辑
         */
        public virtual void Clear()
        {
        }
        /// <summary>
        /// 连接方法
        /// 初始化连接参数并开始连接过程
        /// </summary>
        /// <param name="ip">目标IP地址</param>
        /// <param name="port">目标端口号</param>
        public  void Connect(string ip, int port)
        {
            Init();
            _ip = ip;
            _port = port;
            Connecting();
        }

        /**
         * 连接处理方法
         * 调用异步连接处理器
         */
        public void Connecting()
        {
            AsynConnectHandler();
        }

        /**
         * 关闭连接方法
         * 由子类实现具体的关闭逻辑
         */
        public abstract void Close();

        /**
         * 异步连接处理器
         * 由子类实现具体的异步连接处理逻辑
         */
        protected abstract void AsynConnectHandler();
    }
}