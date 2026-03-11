using System;
using UnityEngine;

namespace Core.Net
{
    public class NetMessage
    {
        public int _cmd;
        public byte[] _data = new byte[0];
        public NetDictionary _body = new NetDictionary();

        public NetDictionary Body
        {
            get { return _body; }
            set { 
                _body = value;
                _data = BodyToBytes(_body);
            }
        }
        
        public NetMessage(int cmd, NetDictionary body)
        {
            this._cmd = cmd;
            this.Body = body;
        }
        
        public NetMessage(int cmd)
        {
            this._cmd = cmd;
        }
        
        public NetMessage Response(NetDictionary body)
        {
            this.Body = body;
            return this;
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
            Debug.Log($"Message ToBytes: cmd={cmdHex}, length={bytes.Length}, content={_body.ToJsonString()}");
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
        
        private static NetDictionary BodyFromBytes(byte[] data)
        {
            // {"key2":{"subKey1":"subValue1","subKey2":"subValue2"},"key1":"value1","key3":"value3"}
            // json字符串，只有嵌套和string类型的value
            String formatString = System.Text.Encoding.UTF8.GetString(data).Trim();
            NetDictionary body = new NetDictionary();
            
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
                    content = (valueEndIndex + 2 >= content.Length) ? "" : content.Substring(valueEndIndex + 2).Trim();
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
        
        public static String DictionaryToJsonString(NetDictionary dict)
        {
            string formatString = "{";
            foreach (var kv in dict)
            {
                formatString += $"\"{kv.Key}\":";
                if (kv.Value is NetDictionary subDict)
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

        private byte[] BodyToBytes(NetDictionary body)
        {
            return System.Text.Encoding.UTF8.GetBytes(DictionaryToJsonString(body));
        }
        

        

        public void PrintLog()
        {
            //Debug.Log($"NetMessage: cmd={_cmd:X4}, body={{ {bodyStr} }}");
            Debug.Log($"NetMessage: cmd={CmdType.TryToGetType(_cmd)}, body={{ {_body.ToJsonString()} }}");
        }
    }
}