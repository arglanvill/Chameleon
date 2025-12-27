using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    public class SignalLine
    {
        public SignalLine(string name,Object owner, bool high)
        {
            Name = name;
            IsHigh = high;
            Connect(owner);
        }
        public void Connect(Object o)
        {
            Connections.Add(o, IsHigh);
        }

        public void SetSignalLow()
        {
            IsHigh = false;
        }

        public void SetSignalHigh()
        {
            IsHigh = true;
        }
        public bool IsSignalLow()
        {
            return !IsHigh;
        }
        public bool IsSignalHigh()
        {
            return IsHigh;
        }

        public bool SignalEdgeHigh(Object o)
        {
            bool previousSignalHigh = Connections[o];
            Connections[o] = IsHigh;
            return IsHigh && !previousSignalHigh;
        }

        public bool SignalEdgeLow(Object o)
        {
            bool previousSignalHigh = Connections[o];
            Connections[o] = IsHigh;
            return !IsHigh && previousSignalHigh;
        }
        public readonly string Name;
        private bool IsHigh;
        private Dictionary<Object, bool> Connections = new Dictionary<Object, bool>();
    }
}
