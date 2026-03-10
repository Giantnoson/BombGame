using System;
using System.Collections.Generic;

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

        public Dictionary<string, object> GetDictionary(string key)
        {
            if (ContainsKey(key))
            {
                return this[key] as Dictionary<string, object>;
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
    }
    
}