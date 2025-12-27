using Chameleon.Emulator.Core;
using Chameleon.Host.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Chameleon.Debugger
{
    class RegisterDebugger : Debugger, IDebugger
    {
        public RegisterDebugger(List<IRegister> registers, Panel parent) : base(parent)
        {
            Registers = registers;
            Source = ToItemsSource(Registers);
            UIPanel = new AccordionPanel();
            UIPanel.Title = "Registers";

            GridView = new GridView();
            GridView.ItemsSource = Source;
            GridView.ItemTemplate = Application.Current.Resources["RegisterItemTemplate"] as DataTemplate;
            UIPanel.Content = GridView;

            UIParent.Children.Add(UIPanel);
        }
        public override void Update(ExecutionState state)
        {
            foreach (ObservableRegister r in Source)
                r.NotifyIfChanged();
        }

        private ObservableCollection<ObservableRegister> ToItemsSource(List<IRegister> registers)
        {
            ObservableCollection<ObservableRegister> source = new ObservableCollection<ObservableRegister>();
            registers.ForEach(r => source.Add(new ObservableRegister(r)));
            return source;
        }

        AccordionPanel UIPanel;
        GridView GridView;
        List<IRegister> Registers;
        ObservableCollection<ObservableRegister> Source;

    }
}
