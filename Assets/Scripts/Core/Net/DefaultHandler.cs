using System;

namespace Core.Net
{
    public class DefaultHandler
    {
        public int CmdType { get; private set; }
        public Action<NetMessage> Handler { get; private set; }
        
        public DefaultHandler(int cmdType, Action<NetMessage> handler)
        {
            CmdType = cmdType;
            Handler = handler;
        }
    }
}