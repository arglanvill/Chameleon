using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    abstract class Component : IComponent
    {
        public List<IRegister> Registers { get; private set; }
        public List<SignalLine> SignalLines { get; private set; }

        public abstract string Name { get; }

        public abstract void PowerOff();
        public abstract void PowerOn();

        protected void AddRegister(IRegister register)
        {
            if (Registers == null)
                Registers = new List<IRegister>();
            Registers.Add(register);
        }
        protected void AddSignalLine(SignalLine line)
        {
            if (SignalLines == null)
                SignalLines = new List<SignalLine>();
            SignalLines.Add(line);
        }
    }
}
