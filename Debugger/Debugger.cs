using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chameleon.Debugger
{
    public abstract class Debugger
    {
        public Debugger(Panel parent)
        {
            UIParent = parent;
        }
        public Panel UIParent { get; }
        public virtual void Update(ExecutionState state)
        {
            foreach (IDebugger debugger in Debuggers)
                debugger.Update(state);
        }
        public List<IDebugger> Debuggers = new List<IDebugger>();

        public Button AddButton(string content, RoutedEventHandler handler, Panel panel)
        {
            Button button = new Button();
            button.Content = content;
            button.Click += handler;
            panel.Children.Add(button);
            return button;
        }

    }
}
