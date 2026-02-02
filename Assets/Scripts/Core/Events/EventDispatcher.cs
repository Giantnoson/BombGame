using System.Collections.Generic;

namespace Core.Events
{

    public class EventDispatcher
    {

        public delegate void OnEventHandler(Event evt);
        private Dictionary<string, List<OnEventHandler>> _listeners = new Dictionary<string, List<OnEventHandler>>();
        //private object _lockListeners = new object();
        public EventDispatcher() { }

        public void AddEventListener(string type, OnEventHandler handler)
        {
            //lock (_lockListeners)
            {
                List<OnEventHandler> list;
                if (_listeners.ContainsKey(type))
                {
                    list = _listeners[type];
                }
                else
                {
                    list = new List<OnEventHandler>();
                    _listeners.Add(type, list);
                }
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == handler)
                    {
                        return;
                    }
                }
                list.Add(handler);    
            }
        }

        public void RemoveEventListener(object target)
        {
            //lock (_lockListeners)
            {
                foreach (KeyValuePair<string, List<OnEventHandler>> kv in _listeners)
                {
                    List<OnEventHandler> list = kv.Value;
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i].Target == target)
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public void RemoveEventListener(string type, OnEventHandler handler)
        {
            //lock (_lockListeners)
            {
                if (_listeners.ContainsKey(type))
                {
                    List<OnEventHandler> list = _listeners[type];
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i] == handler)
                        {
                            list.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        public virtual void RemoveAllListener()
        {
            _listeners.Clear();
        }

        public virtual void DispatchEvent(Event evt)
        {
            if (evt != null)
            {
                //evt.target = this;
                //lock (_lockListeners)
                {
                    if (_listeners.ContainsKey(evt.type))
                    {
                        List<OnEventHandler> list = _listeners[evt.type];
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            list[i](evt);
                        }
                    }
                }
            }
        }
    }
}
