using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Event = Core.Events.Event;

namespace Core.Net
{
    // 基于 AbsTcpClient 的 TCP 实现，使用 4 字节长度前缀进行消息分帧
    public class TcpClientImpl : AbsTcpClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public Action OnConnected;
        public Action<Exception> OnError;
        public Action OnClosed;
        public Action<byte[]> OnMessageRaw;

        public override void Clear()
        {
            msgList.Clear();
            _eventQueues = new Queue<Event>();
        }

        protected override void AsynConnectHandler()
        {
            // 启动异步连接流程
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(_ip, _port).ConfigureAwait(false);
                    _stream = _client.GetStream();

                    OnConnected?.Invoke();

                    // 开始读取循环
                    await ReadLoopAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                    Close();
                }
            }, _cts.Token);
        }

        private async Task ReadExactAsync(byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int read = 0;
            while (read < count)
            {
                int n = await _stream.ReadAsync(buffer, offset + read, count - read, ct).ConfigureAwait(false);
                if (n == 0) throw new Exception("Remote closed");
                read += n;
            }
        }

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

        // 发送原始消息（会添加 4 字节长度前缀）
        private void SendBytes(byte[] data)
        {
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

        // 便捷方法：发送 Message 对象（依赖项目中的 Message 类）
        public void SendMessage(NetMessage msg)
        {
            SendBytes(msg.ToBytes());
        }
    }
}
