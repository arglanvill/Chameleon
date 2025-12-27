using Chameleon.Debugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    public interface ISystem
    {
        void PowerOn();
        void PowerOff();
        bool IsPoweredOn { get; }
        void UpdateFrame();
        void DrawFrame(params Object[] drawParams);
        void Step();
        string Name { get; }
        List<IComponent> Components { get; }
        SystemDebugger Debugger { get; set; }

    }
}
