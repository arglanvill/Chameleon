using Chameleon.Emulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Chameleon.Debugger
{
    public class SystemDebugger : Debugger
    {
        public SystemDebugger(Panel parent, CoreDispatcher dispatcher) : base(parent)
        {
            Dispatcher = dispatcher;
        }
        public void InitializeDebugger()
        {
            Debuggers.Add(new ExecutionController(this, UIParent));
            foreach (IComponent component in EmulatedSystem.Components)
                Debuggers.Add(new ComponentDebugger(component, UIParent));
        }

        ISystem _EmulatedSystem;
        public ISystem EmulatedSystem
        {
            get => _EmulatedSystem;
            set
            {
                _EmulatedSystem = value;
                if (_EmulatedSystem != null)
                    InitializeDebugger();
            }
        }

        public void UpdateFrame()
        {
            if (State == ExecutionState.Running)
                EmulatedSystem.UpdateFrame();
            else if (State == ExecutionState.Stepping)
            {
                EmulatedSystem.Step();
                State = ExecutionState.Stopped;
            }
            Update(State);
        }
        public override void Update(ExecutionState state)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => base.Update(state));
        }
        CoreDispatcher Dispatcher;

        public ExecutionState State = ExecutionState.Running;
    }
}
