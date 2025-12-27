using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Debugger
{
    public enum ExecutionState
    {
        Running,
        Stopped,
        Stepping
    }

    public interface IDebugger
    {
        void Update(ExecutionState state);
    }
}
