using System;
using System.Collections.Generic;
using System.Linq;
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
            return "";
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
            // string cmdHex = _cmd.ToString("X4");
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
            // {"key2":{"subKey1":"subValue1","subKey2":"subValue2"},"key1":"value1","key3":"value3"}
            // json字符串，只有嵌套和string类型的value
            String formatString = System.Text.Encoding.UTF8.GetString(data).Trim();
            Dictionary<String, Object> body = new Dictionary<String, Object>();
            if (!(formatString.StartsWith("{") && formatString.EndsWith("}")))
            {
                Debug.LogError($"Invalid format string: {formatString}");
                return body;
            }
            String content = formatString.Substring(1, formatString.Length - 2);
            if (content == "")
            {
                return body;
            }

            while (content.Length > 0)
            {
                int keyEndIndex = content.IndexOf(":", StringComparison.Ordinal);
                if (keyEndIndex == -1)
                    throw new FormatException($"Invalid format string: {formatString}");
                String key = content.Substring(0, keyEndIndex).Trim().Trim('"');
                content = content.Substring(keyEndIndex + 1).Trim();
                if (content.StartsWith("{"))
                {
                    int valueEndIndex = findMatchBraceIndex(0, content);
                    String valueStr = content.Substring(0, valueEndIndex + 1);
                    body[key] = BodyFromBytes(System.Text.Encoding.UTF8.GetBytes(valueStr));
                    content = content.Substring(valueEndIndex + 2).Trim();
                }
                else
                {
                    int valueEndIndex = content.IndexOf(",", StringComparison.Ordinal);
                    String valueStr;
                    if (valueEndIndex == -1)
                    {
                        valueStr = content;
                        content = "";
                    }
                    else
                    {
                        valueStr = content.Substring(0, valueEndIndex);
                        content = content.Substring(valueEndIndex + 1).Trim();
                    }
                    body[key] = valueStr.Trim().Trim('"');
                }
            }
            return body;
        }

        private static int findMatchBraceIndex(int idx, String content)
        {
            int braceCount = 0;
            for (int i = idx; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    braceCount++;
                }
                else if (content[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        return i;
                    }
                }
            }
            throw new FormatException($"No matching brace found in content: {content}");
        }
        
        private String DictionaryToJsonString(Dictionary<String, Object> dict)
        {
            string formatString = "{";
            foreach (var kv in dict)
            {
                formatString += $"\"{kv.Key}\":";
                if (kv.Value is Dictionary<String, Object> subDict)
                {
                    formatString += DictionaryToJsonString(subDict);
                }
                else
                {
                    formatString += $"\"{kv.Value}\"";
                }

                formatString += ",";
            }

            if (formatString.EndsWith(","))
            {
                formatString = formatString.Substring(0, formatString.Length - 1);
            }

            return formatString + "}";
        }

        private byte[] BodyToBytes(Dictionary<String, Object> body)
        {
            return System.Text.Encoding.UTF8.GetBytes(DictionaryToJsonString(body));
        }

        public void PrintLog()
        {
            string bodyStr = string.Join(", ", _body.Select(kv => $"{kv.Key}={kv.Value}"));
            //Debug.Log($"NetMessage: cmd={_cmd:X4}, body={{ {bodyStr} }}");
            Debug.Log($"NetMessage: cmd={CmdType.TryToGetType(_cmd)}, body={{ {bodyStr} }}");
        }
    }
}