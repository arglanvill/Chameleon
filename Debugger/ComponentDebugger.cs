using Chameleon.Emulator.Core;
using Chameleon.Host.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WebUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chameleon.Debugger
{
    class ComponentDebugger : Debugger, IDebugger
    {
        public ComponentDebugger(IComponent component, Panel parent) : base(parent)
        {
            UIPanel = new AccordionPanel();
            UIPanel.Title = component.Name;
            ContentPanel = new StackPanel();
            UIPanel.Content = ContentPanel;
            if (component.Registers.Count > 0)
                Debuggers.Add(new RegisterDebugger(component.Registers, ContentPanel));
            UIParent.Children.Add(UIPanel);
        }

        AccordionPanel UIPanel;
        StackPanel ContentPanel;
    }
}
