using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu
{ 
    public class MainUIManager : MonoBehaviour
    {
        private static MainUIManager _instance;
        public static MainUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MainUIManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MainUIManager");
                        _instance = go.AddComponent<MainUIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, UIBasePanel> _panels = new Dictionary<string, UIBasePanel>();
        private Stack<UIBasePanel> _panelStack = new Stack<UIBasePanel>();

        private List<string> _dontHide = new List<string>();//只有closeALL可以关闭

        public List<string> DontHide
        {
            get => _dontHide;
            set => _dontHide = value;
        }

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
        
        public void ShowPanels(List<string> panelName)
        {
            TryGetValues(panelName, out List<UIBasePanel> panels);
            while (_panelStack.Count > 0)
            {
                _panelStack.Peek().Hide();
            }

            foreach (var panel in panels)
            {
                panel.Show();
                _panelStack.Push(panel);
            }
        }

        public void ShowDontHidePanel(List<string> panelName)
        {
            TryGetValues(panelName, out List<UIBasePanel> panels);
            foreach (var panel in panels)
            {
                panel.Show();
            }
        }
        
        public void ShowDontHidePanel(string panelName)
        {
            if (_panels.TryGetValue(panelName, out UIBasePanel panel))
            {
                panel.Show();
            }
            else
            {
                Debug.LogWarning($"Panel {panelName} not found!");
            }
        }

        private void TryGetValues(List<string> panelNames ,out List<UIBasePanel> panels)
        {
            panels = new List<UIBasePanel>();
            foreach (var panelName in panelNames)
            {
                if (_panels.TryGetValue(panelName, out UIBasePanel panel))
                {
                    panels.Add(panel);
                }
                else
                {
                    Debug.LogWarning($"Panel {panelName} not found!");
                }
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
