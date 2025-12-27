using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    public interface IComponent
    {
        void PowerOn();
        void PowerOff();
        List<IRegister> Registers { get; }
        List<SignalLine> SignalLines { get; }
        string Name { get; }
    }
}
