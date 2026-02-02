using System.Collections.Generic;
using Core.Events;

namespace Core.Net
{
    public abstract class AbsTcpClient : EventDispatcher
    {
        public string _ip;
        public int _port;

        protected Queue<Event> _eventQueues;
        public Dictionary<int, int> msgList = new Dictionary<int, int>();

        public void Init()
        {
            Clear();
        }

        virtual public void Clear()
        {
        }
        
        virtual public void Connect(string ip, int port)
        {
            Init();
            _ip = ip;
            _port = port;
            Connecting();
        }

        public void Connecting()
        {
            AsynConnectHandler();
        }

        abstract public void Close();

        abstract protected void AsynConnectHandler();
    }
}