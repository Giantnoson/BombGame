using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, UIBasePanel> _panels = new Dictionary<string, UIBasePanel>();
        private Stack<UIBasePanel> _panelStack = new Stack<UIBasePanel>();

        public void RegisterPanel(string panelName, UIBasePanel panel)
        {
            if (!_panels.ContainsKey(panelName))
            {
                _panels.Add(panelName, panel);
            }
        }

        public T GetPanel<T>(string panelName) where T : UIBasePanel
        {
            if (_panels.TryGetValue(panelName, out UIBasePanel panel))
            {
                return panel as T;
            }
            return null;
        }

        public void ShowPanel(string panelName)
        {
            if (_panels.TryGetValue(panelName, out UIBasePanel panel))
            {
                if (_panelStack.Count > 0)
                {
                    _panelStack.Peek().Hide();
                }

                panel.Show();
                _panelStack.Push(panel);
            }
            else
            {
                Debug.LogWarning($"Panel {panelName} not found!");
            }
        }

        public void Back()
        {
            if (_panelStack.Count > 1)
            {
                UIBasePanel current = _panelStack.Pop();
                current.Hide();

                UIBasePanel previous = _panelStack.Peek();
                previous.Show();
            }
        }

        public void CloseAll()
        {
            foreach (var panel in _panels.Values)
            {
                panel.Hide();
            }
            _panelStack.Clear();
        }
    }
}
