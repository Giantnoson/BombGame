using System;
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

        public void ShowPanel(PanelSymbol symbol)
        {
            string panelName = symbol.name;
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
        
        public void ShowPanels(List<PanelSymbol> symbols)
        {
            TryGetValues(symbols, out List<UIBasePanel> panels);
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

        public void ShowDontHidePanel(List<PanelSymbol> symbols)
        {
            TryGetValues(symbols, out List<UIBasePanel> panels);
            foreach (var panel in panels)
            {
                panel.Show();
            }
        }
        
        public void ShowDontHidePanel(PanelSymbol symbol)
        {
            string panelName = symbol.name;
            if (_panels.TryGetValue(panelName, out UIBasePanel panel))
            {
                panel.Show();
            }
            else
            {
                Debug.LogWarning($"Panel {panelName} not found!");
            }
        }

        private void TryGetValues(List<PanelSymbol> symbols ,out List<UIBasePanel> panels)
        {
            panels = new List<UIBasePanel>();
            foreach (var symbol in symbols)
            {
                string panelName = symbol.name;
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