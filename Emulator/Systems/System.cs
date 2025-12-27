using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chameleon.Emulator.Core;
using Chameleon.Host;

namespace Chameleon.Emulator.Systems
{
    abstract class System : ISystem
    {
        public System(Chameleon.Host.Host host)
        {
            Host = host;
        }
        protected Chameleon.Host.Host Host;
        public abstract bool IsPoweredOn { get; }
        public abstract string Name { get; }

        public List<IComponent> Components { get; private set; }
        public abstract Debugger.SystemDebugger Debugger { get; set; }

        protected void AddComponent(IComponent component)
        {
            if (Components == null)
                Components = new List<IComponent>();
            Components.Add(component);
        }

        public abstract void PowerOn();
        public abstract void PowerOff();
        public abstract void UpdateFrame();
        public abstract void DrawFrame(params Object[] drawParams);
        public abstract void Step();
    }
}
