using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Core.Net
{
    public class NetMessage
    {
        public int _cmd;
        public byte[] _data = new byte[0];
        public Dictionary<String, Object> _body = new Dictionary<String, Object>();

        public Dictionary<String, Object> Body
        {
            get { return _body; }
            set { 
                _body = value;
                _data = BodyToBytes(_body);
            }
        }
        
        public NetMessage(int cmd, Dictionary<String, Object> body)
        {
            this._cmd = cmd;
            this.Body = body;
        }
        
        public NetMessage(int cmd)
        {
            this._cmd = cmd;
        }
        
        public NetMessage Response(Dictionary<String, Object> body)
        {
            this.Body = body;
            return this;
        }
        
        public string GetString(string key)
        {
            if (_body.ContainsKey(key))
            {
                return _body[key].ToString();
            }
            return null;
        }
        
        public int GetInt(string key)
        {
            if (_body.ContainsKey(key))
            {
                if (int.TryParse(_body[key].ToString(), out int result))
                {
                    return result;
                }
            }
            return 0;
        }
        
        public float GetFloat(string key)
        {
            if (_body.ContainsKey(key))
            {
                if (float.TryParse(_body[key].ToString(), out float result))
                {
                    return result;
                }
            }
            return 0f;
        }
        
        public bool GetBool(string key)
        {
            if (_body.ContainsKey(key))
            {
                if (bool.TryParse(_body[key].ToString(), out bool result))
                {
                    return result;
                }
            }
            return false;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[_data.Length + 4];
            // 转为网络字节序
            int cmdNetworkOrder = System.Net.IPAddress.HostToNetworkOrder(_cmd);
            byte[] cmdBytes = BitConverter.GetBytes(cmdNetworkOrder);
            Array.Copy(cmdBytes, 0, bytes, 0, 4);
            Array.Copy(_data, 0, bytes, 4, _data.Length);
            // 转为16进制字符串输出调试
            string cmdHex = _cmd.ToString("X4");
            // Debug.Log($"Message ToBytes: cmd={cmdHex}, length={bytes.Length}");
            return bytes;
        }
        
        public static NetMessage FromBytes(byte[] bytes)
        {
            int cmd = BitConverter.ToInt32(bytes, 0);
            // 转为主机字节序
            int cmdHost = System.Net.IPAddress.NetworkToHostOrder(cmd);
            byte[] data = new byte[bytes.Length - 4];
            Array.Copy(bytes, 4, data, 0, data.Length);
            return new NetMessage(cmdHost, BodyFromBytes(data));
        }
        
        private static Dictionary<String, Object> BodyFromBytes(byte[] data)
        {
            // 目前格式是 playerId=111234;userName=TestUser;score=1000 之类的字符串
            String formatString = System.Text.Encoding.UTF8.GetString(data);
            String[] pairs = formatString.Split(';');
            Dictionary<String, Object> body = new Dictionary<String, Object>();
            foreach (String pair in pairs)
            {
                String[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    body[keyValue[0]] = keyValue[1];
                }
            }
            return body;
        }
        
        private byte[] BodyToBytes(Dictionary<String, Object> body)
        {
            List<String> pairs = new List<String>();
            foreach (KeyValuePair<String, Object> kv in body)
            {
                pairs.Add($"{kv.Key}={kv.Value}");
            }
            String formatString = String.Join(";", pairs);
            return System.Text.Encoding.UTF8.GetBytes(formatString);
        }
    }
}