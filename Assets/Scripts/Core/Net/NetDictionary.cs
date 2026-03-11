using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Net
{
    public class NetDictionary : Dictionary<String, Object>
    {
        public string GetString(string key)
        {
            if (ContainsKey(key))
            {
                return this[key].ToString();
            }
            return "";
        }

        public NetDictionary GetDictionary(string key)
        {
            if (ContainsKey(key))
            {
                return this[key] as NetDictionary;
            }
            return null;
        }
        
        public int GetInt(string key)
        {
            if (ContainsKey(key))
            {
                if (int.TryParse(this[key].ToString(), out int result))
                {
                    return result;
                }
            }
            return 0;
        }
        
        public float GetFloat(string key)
        {
            if (ContainsKey(key))
            {
                if (float.TryParse(this[key].ToString(), out float result))
                {
                    return result;
                }
            }
            return 0f;
        }
        
        public bool GetBool(string key)
        {
            if (ContainsKey(key))
            {
                if (bool.TryParse(this[key].ToString(), out bool result))
                {
                    return result;
                }
            }
            return false;
        }
        
        public string ToJsonString(NetDictionary body = null)
        {
            body ??= this;
            var ret = new StringBuilder("{");
    
            foreach (var kv in body)
            {
                string k = kv.Key;
                object v = kv.Value;

                if (v is NetDictionary)
                {
                    // 如果值是 MessageBody 类型，递归调用 ToJsonString
                    ret.Append($"\"{k}\":{ToJsonString(v as NetDictionary)}");
                }
                else
                {
                    // 否则，将值转换为字符串并加上双引号
                    ret.Append($"\"{k}\":\"{v}\"");
                }
        
                ret.Append(",");
            }

            // 如果字典不为空，删除最后一个逗号
            if (body.Count > 0)
            {
                ret.Remove(ret.Length - 1, 1);
            }

            ret.Append("}");
            return ret.ToString();
        }
    }
    
}