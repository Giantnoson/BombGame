using System.Collections.Generic;

namespace Core.Net
{
    public class WrappedDict : Dictionary<string, string>
    {
        public string GetString(string key)
        {
            return this[key];
        }
        
        public int GetInt(string key)
        {
            return int.Parse(this[key]);
        }

        public float GetFloat(string key)
        {
            return float.Parse(this[key]);
        }
        
        public bool GetBool(string key)
        {
            return bool.Parse(this[key]);
        }
    }
}