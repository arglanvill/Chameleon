using Chameleon.Emulator.Core;
using Chameleon.Host.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chameleon.Debugger
{
    class ExecutionController : Debugger, IDebugger
    {
        public ExecutionController(SystemDebugger debugger, Panel parent) : base(parent)
        {
            Debugger = debugger;

            UIPanel = new AccordionPanel();
            UIPanel.Title = "Execution";
            UIParent.Children.Add(UIPanel);

            StackPanel = new StackPanel();
            UIPanel.Content = StackPanel;
            StackPanel.Orientation = Orientation.Horizontal;

            // Add buttons for Run, Break, Stop, Step
            Run = AddButton("Run", Run_Click, StackPanel);
            Stop = AddButton("Stop", Stop_Click, StackPanel);
            Break = AddButton("Break", Break_Click, StackPanel);
            Step = AddButton("Step", Step_Click, StackPanel);
        }

        SystemDebugger Debugger;
        AccordionPanel UIPanel;
        StackPanel StackPanel;
        Button Run;
        Button Break;
        Button Stop;
        Button Step;

        public override void Update(ExecutionState state)
        {
            switch (state)
            {
                case ExecutionState.Running:
                    Run.IsEnabled = Step.IsEnabled = false;
                    Stop.IsEnabled = Break.IsEnabled = true;
                    break;
                case ExecutionState.Stopped:
                    Run.IsEnabled = Step.IsEnabled = Stop.IsEnabled = true;
                    Break.IsEnabled = false;
                    break;
                case ExecutionState.Stepping:
                    Run.IsEnabled = Stop.IsEnabled = Step.IsEnabled = true;
                    Break.IsEnabled = false;
                    break;
            }
        }

        private void Run_Click(Object sender, RoutedEventArgs e)
        {
            if (Debugger.EmulatedSystem.IsPoweredOn == false)
                Debugger.EmulatedSystem.PowerOn();
            Debugger.State = ExecutionState.Running;
        }
        private void Stop_Click(Object sender, RoutedEventArgs e)
        {
            if (Debugger.EmulatedSystem.IsPoweredOn)
                Debugger.EmulatedSystem.PowerOff();
            Debugger.State = ExecutionState.Stopped;
        }
        private void Break_Click(Object sender, RoutedEventArgs e)
        {
            Debugger.State = ExecutionState.Stopped;
        }
        private void Step_Click(Object sender, RoutedEventArgs e)
        {
            Debugger.State = ExecutionState.Stepping;
        }
    }
}
