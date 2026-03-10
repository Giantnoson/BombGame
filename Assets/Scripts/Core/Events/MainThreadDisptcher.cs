using System;
using System.Collections.Generic;
using Core.Net;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static long _lasetHeartbeatTime = 0;

    public static void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    /// <summary>
    /// 更新方法，用于执行队列中的任务并发送心跳消息
    /// </summary>
    public void Update()
    {
        // 发送一个类型为心跳的网络消息
        if (Time.time - _lasetHeartbeatTime > 5 && TcpGameClient.IsLogin && TcpGameClient.Instance.IsConnected)
        {
            _lasetHeartbeatTime = (long)Time.time;
            TcpGameClient.SendMessage(new NetMessage(CmdType.Heartbeat));
            //print("发送心跳");
        }
        
        
        // 使用锁确保对执行队列的操作是线程安全的
        lock (_executionQueue)
        {
            // 当队列中还有任务时，循环执行
            while (_executionQueue.Count > 0)
            {
                // 从队列中取出一个任务并执行，如果任务不为null
                _executionQueue.Dequeue()?.Invoke();
            }
        }
        
        
    }
}