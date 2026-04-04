using System.Collections.Generic;
using GameSystem.GameScene.MainMenu;
using GameSystem.UI;
using UnityEngine;

namespace GameSystem.GameScene
{
    public abstract class BaseUIManager : MonoBehaviour
    {
        private Dictionary<string, UIBasePanel> _panels = new Dictionary<string, UIBasePanel>();
        private Stack<UIBasePanel> _panelStack = new Stack<UIBasePanel>();

        private List<string> _dontHide = new List<string>();//只有closeALL可以关闭
        
        private string resourcePath = "GUI/";
        
        public List<string> DontHide
        {
            get => _dontHide;
            set => _dontHide = value;
        }

        public void RegisterPanel(UIBasePanel panel)
        {
            string panelName = panel.symbol.name;
            if (!_panels.ContainsKey(panelName))
            {
                _panels.Add(panelName, panel);
            }
        }

        public T GetPanel<T>(PanelSymbol symbol) where T : UIBasePanel
        {
            string panelName = symbol.name;
            if (_panels.TryGetValue(panelName, out UIBasePanel panel))
            {
                return panel as T;
            }
            return null;
        }

        public void ShowPanel(PanelSymbol symbol, bool dontHide = false, Dictionary<string, string> parameters = null)
        {
            string panelName = symbol.name;
            if (!_panels.ContainsKey(panelName))
            {
                GameObject panelPrefab = Resources.Load<GameObject>(resourcePath + panelName);
                if (panelPrefab == null)
                {
                    Debug.LogError($"Panel prefab {panelName} not found at path {resourcePath + panelName}!");
                    return;
                }
                GameObject panelObj = Instantiate(panelPrefab, transform);
                MainUIController.AddPanel(panelObj);
                UIBasePanel newPanel = panelObj.GetComponent<UIBasePanel>();
                if (newPanel == null)
                {
                    Debug.LogError($"Panel prefab {panelName} does not have a UIBasePanel component!");
                    Destroy(panelObj);
                    return;
                }
                RegisterPanel(newPanel);
                newPanel.OnCreate(parameters);
            }
            if (_panels.TryGetValue(panelName, out UIBasePanel panel))
            {
                if (_panelStack.Count > 0)
                {
                    _panelStack.Peek().Hide();
                }

                panel.Show();
                if (!dontHide)
                {
                    _panelStack.Push(panel);
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

        public void BackForDontHide()
        {
            if (_panelStack.Count > 1)
            {
                UIBasePanel current = _panelStack.Peek();
                current.Show();
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