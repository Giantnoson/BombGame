using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Event = Core.Events.Event;

namespace Core.Net
{
    /// <summary>
    /// TcpClientImpl 类实现了基于TCP的客户端通信功能，继承自 AbsTcpClient 抽象类，使用 4 字节长度前缀进行消息分帧
    /// 提供了异步连接、消息收发、错误处理等功能。
    /// </summary>
    public class TcpClientImpl : AbsTcpClient
    {
        // TCP客户端实例
        private TcpClient _client;
        // 网络流，用于读写数据
        private NetworkStream _stream;
        // 用于取消异步操作的通知源
        private CancellationTokenSource _cts;

        // 连接成功回调
        public Action OnConnected;
        // 错误发生回调
        public Action<Exception> OnError;
        // 连接关闭回调
        public Action OnClosed;
        // 接收到原始消息回调
        public Action<byte[]> OnMessageRaw;

        /// <summary>
        /// 清理资源，重置消息列表和事件队列
        /// </summary>
        public override void Clear()
        {
            msgList.Clear();
            _eventQueues = new Queue<Event>();
        }

        /// <summary>
        /// 异步连接处理方法，启动连接流程并开始读取循环
        /// </summary>
        protected override void AsynConnectHandler()
        {
            // 启动异步连接流程
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    _client = new TcpClient();
                    // 异步连接到指定IP和端口
                    await _client.ConnectAsync(_ip, _port).ConfigureAwait(false);
                    // 获取网络流
                    _stream = _client.GetStream();

                    // 触发连接成功回调
                    OnConnected?.Invoke();

                    // 开始读取循环
                    await ReadLoopAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 触发错误回调并关闭连接
                    OnError?.Invoke(ex);
                    Close();
                }
            }, _cts.Token);
        }

        /// <summary>
        /// 异步读取指定长度的数据
        /// </summary>
        /// <param name="buffer">存储数据的缓冲区</param>
        /// <param name="offset">缓冲区偏移量</param>
        /// <param name="count">要读取的字节数</param>
        /// <param name="ct">取消令牌</param>
        private async Task ReadExactAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int read = 0;
            while (read < count)
            {
                // 异步读取数据
                int n = await _stream.ReadAsync(buffer, offset + read, count - read, ct).ConfigureAwait(false);
                if (n == 0) throw new Exception("Remote closed");
                read += n;
            }
        }

        /// <summary>
        /// 消息读取循环，持续接收并处理消息
        /// </summary>
        /// <param name="ct">取消令牌</param>
        private async Task ReadLoopAsync(CancellationToken ct)
        {
            try
            {
                byte[] lenBuf = new byte[4];
                while (!ct.IsCancellationRequested && _client != null && _client.Connected)
                {
                    // Debug.Log("waiting for message via TCP");
                    // 读取 4 字节长度前缀（network order, big-endian）
                    await ReadExactAsync(lenBuf, 0, 4, ct).ConfigureAwait(false);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(lenBuf);
                    int len = BitConverter.ToInt32(lenBuf, 0);
                    if (len <= 0 || len > 1 * 1024 * 1024) // 限制最大消息大小（10MB）
                        throw new Exception($"Invalid message length: {len}");

                    // Debug.Log("receive message via TCP, length=" + len);
                    byte[] payload = new byte[len];
                    await ReadExactAsync(payload, 0, len, ct).ConfigureAwait(false);

                    // 调用主线程处理接收到的原始字节
                    var copy = payload;
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        try
                        {
                            OnMessageRaw?.Invoke(copy);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"OnMessageRaw handler error: {e}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // 连接意外中断
                MainThreadDispatcher.Enqueue(() => OnError?.Invoke(ex));
            }
            finally
            {
                Close();
            }
        }

        public override void Close()
        {
            try
            {
                _cts?.Cancel();
            }
            catch { }

            try
            {
                _stream?.Close();
            }
            catch { }

            try
            {
                _client?.Close();
            }
            catch { }

            _stream = null;
            _client = null;
            _cts = null;

            MainThreadDispatcher.Enqueue(() => OnClosed?.Invoke());
        }

        /// <summary>
        /// 发送原始消息（会添加 4 字节长度前缀）
        /// </summary>
        /// <param name="data">传输的数据</param>
        /// <exception cref="InvalidOperationException">抛出连接异常</exception>
        private void SendBytes(byte[] data)
        {
            // 检查客户端是否连接，如果未连接则抛出异常
            if (_client == null || !_client.Connected) throw new InvalidOperationException("Not connected");
            // length prefix big-endian
            byte[] lenBuf = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(lenBuf);
            // 合并并发送（同线程）
            try
            {
                _stream.Write(lenBuf, 0, 4);
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        /// <summary>
        ///  便捷方法：发送 Message 对象（依赖项目中的 Message 类）
        /// </summary>
        /// <param name="msg">发送的消息</param>
        public void SendMessage(NetMessage msg)
        {
            SendBytes(msg.ToBytes());
        }
    }
}
